using ASU_U_Operator.Configuration;
using ASU_U_Operator.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    public class CoreInitializer : ICoreInitializer
    {
        private readonly IPreparedAppConfig _appConfig;
        private readonly ILogger<CoreInitializer> _log;
        private readonly IWorkerService _workerService;
        private readonly IHealthcheck _healthcheck;
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> cancelContexts;
        private List<IPluginWorker> plugins;
        private IEnumerable<Guid> pluginsGuid => plugins?.Select(x => x.Key);

        public CoreInitializer(IPreparedAppConfig appConfig,
            ILogger<CoreInitializer> log,
            IWorkerService workerService,
            IHealthcheck healthcheck
            )
        {
            _appConfig = appConfig;
            _log = log;

            cancelContexts = new ConcurrentDictionary<Guid, CancellationTokenSource>();
            _workerService = workerService ?? throw new InvalidProgramException("WorkerService not defined");
            _healthcheck = healthcheck ?? throw new InvalidProgramException("Healthcheck not defined");
        }

        public IEnumerable<Guid> LoadAll()
        {
            _workerService.ResetAll();

            plugins = LoadFromConfig();

            foreach (var plugin in plugins)
            {
                _workerService.AddOrUpdate(plugin);
                _log.LogInformation($"Load plugin {plugin.Info()}..");
            }
            return pluginsGuid;
        }

        public IEnumerable<Guid> AttachByNewOptions(OperatorSection operatorOptions)
        {
            if (operatorOptions == null)
            {
                throw new Exception("Config settings is null");
            }
            var new_plugins = LoadFromConfig(operatorOptions);
            //уже загруженные плагины пропускаем, потому что они могут работать, а те которые еще небыли в работе  - загружаем
            for (int i = 0; i < new_plugins.Count; i++)
            {
                if (plugins.Any(x => x.Key == new_plugins[i].Key))
                {
                    new_plugins[i] = null;
                }
            }

            List<Guid> pluginsKeys = new List<Guid>();

            for (int i = 0; i < new_plugins.Count; i++)
            {
                if (new_plugins[i] != null)
                {
                    _workerService.AddOrUpdate(new_plugins[i]);
                    plugins.Add(new_plugins[i]);
                    _log.LogInformation($"Load plugin {new_plugins[i].Info()}..");
                    pluginsKeys.Add(new_plugins[i].Key);
                }
            }
            return pluginsKeys;
        }

        private List<IPluginWorker> LoadFromConfig(OperatorSection operatorOptions = null)
        {
            if (operatorOptions == null)
            {
                operatorOptions = _appConfig.Operator;
            }

            List<IPluginWorker> pluginsLibs = new List<IPluginWorker>();

            var pluginsOptions = operatorOptions.plugins;

            //https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/creating-app-with-plugin-support
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
            if (pluginsOptions != null)
            {
                var throwIfPluginNotFound = operatorOptions.sys.throwIfPluginNotFound.HasValue ? operatorOptions.sys.throwIfPluginNotFound.Value : true;
                foreach (var plg in pluginsOptions)
                {
                    try
                    {
                        var pluginPath = Path.Combine(rootPath, plg.path);
                        if (File.Exists(pluginPath))
                        {
                            Assembly pluginAssembly = LoadPlugin(pluginPath);
                            var ws = CreateWorkers(pluginAssembly);
                            pluginsLibs.AddRange(ws);
                        }
                        else
                        {
                            throw new Exception($"Plugin [{plg.header}] not found. throwIfPluginNotFound={throwIfPluginNotFound}.PluginPath={pluginPath} ");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (throwIfPluginNotFound)
                        {
                            throw ex;
                        }
                        _log.LogWarning(ex.ToString());
                    }
                }
            }
            return pluginsLibs;
        }

        private Assembly LoadPlugin(string relativePath)
        {
            return Assembly.LoadFrom(relativePath);
        }

        private IEnumerable<IPluginWorker> CreateWorkers(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPluginWorker).IsAssignableFrom(type))
                {
                    IPluginWorker result = Activator.CreateInstance(type) as IPluginWorker;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements IWorker in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }

        public bool RunPlugin(Guid pluginKey, CancellationToken stoppingToken)
        {
            //У HostedService нет unhandledException, поэтому не получается организовать единый узел обработки
            IPluginWorker plugin = null;
            try
            {
                _log.LogInformation($"Run plugin [{pluginKey}]..");

                plugin = plugins.FirstOrDefault(x => x.Key == pluginKey);

                if (cancelContexts.ContainsKey(pluginKey))
                {
                    throw new Exception($"Plugin already run! Plugin {plugin.Info()}");
                }

                if (plugin == null)
                {
                    _log.LogWarning($"Plugin [{pluginKey}] not loaded");
                    return false;
                }

                PreparePlugin(plugin);
                StartPlugin(plugin, stoppingToken);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.ToString());
                _log.LogError("Plugin {0} not run.", plugin == null ? pluginKey.ToString() : plugin.Info());
                return false;
            }
        }

        private void StartPlugin(IPluginWorker plugin, CancellationToken stoppingToken)
        {
            if (cancelContexts.ContainsKey(plugin.Key))
            {
                throw new Exception($"CancelContexts: key already exist! Plugin {plugin.Info()}");
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            if (cancelContexts.TryAdd(plugin.Key, cts))
            {
                plugin.Start(cts.Token);
            }
        }

        private void PreparePlugin(IPluginWorker plugin)
        {
            var throwIfInitError = _appConfig.Operator.sys.throwIfInitError ?? false;
            var init = InitPlugin(plugin);
            if (!init.IsCompletedSuccessfully)
            {
                Exception ex = init.Exception ?? new Exception($"plugin {plugin.Info()} initialize with errors");
                _log.LogError(ex.ToString());
                if (throwIfInitError)
                {
                    throw ex;
                }
            }
            else
            {
                _workerService.MarkInit(plugin.Key, true);
                _log.LogInformation($"plugin {plugin.Info()} successfully initialize.");
                //после инициализации плагин должен положительно отвечать на healthcheck
                if (_appConfig.Operator.sys.enableHealthcheck ?? false)
                {
                    //healthcheck
                    _healthcheck.RunNew(plugin);
                }
            }
        }

        private Task InitPlugin(IPluginWorker plugin)
        {
            _log.LogInformation($"Init plugin {plugin.Name}({plugin.Key})");
            var timeout = _appConfig.Operator.sys.pluginShutdownTimeoutMs ?? 5000;

            var init = plugin.Init();

            if (Task.WhenAny(init, Task.Delay(timeout)).Result != init)
            {
                Task.FromException(new Exception($"Init plugin timeout expired! Plugin: {plugin.Info()}"));
            }
            return init;
        }

        public bool StopPlugin(Guid pluginKey)
        {
            _log.LogInformation($"Stopping plugin {pluginKey}");

            IPluginWorker plugin = null;
            try
            {
                plugin = plugins.FirstOrDefault(x => x.Key == pluginKey);
                if (plugin == null)
                {
                    _log.LogWarning($"Plugin [{pluginKey}] not loaded");
                    return false;
                }

                if (!cancelContexts.ContainsKey(pluginKey))
                {
                    throw new Exception($"Plugin not run! Plugin {plugin.Info()}");
                }

                var timeout = _appConfig.Operator.sys.pluginShutdownTimeoutMs ?? 5000;

                _healthcheck.Stop(plugin.Key);

                CancellationTokenSource cts;
                if (cancelContexts.TryRemove(plugin.Key, out cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }

                var stop = plugin.Stop();
                if (Task.WhenAny(stop, Task.Delay(timeout)).Result != stop)
                {
                    throw new Exception($"Stoping timeout expired! Plugin: {plugin.Info()}");
                }

                _workerService.MarkInit(plugin.Key, false);

                if (stop.Exception != null)
                {
                    throw stop.Exception;
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.ToString());
                return false;
            }
        }
    }
}