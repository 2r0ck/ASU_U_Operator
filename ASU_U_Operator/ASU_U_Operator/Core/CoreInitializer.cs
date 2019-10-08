using ASU_U_Operator.Configuration;
using ASU_U_Operator.Model;
using ASU_U_Operator.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    public class CoreInitializer : ICoreInitializer
    {
        private readonly IPreparedAppConfig _appConfig;
        private readonly ILogger<CoreInitializer> _log;
        private readonly IWorkerService _workerService;

        public CoreInitializer(IPreparedAppConfig appConfig,
            ILogger<CoreInitializer> log,
            IWorkerService workerService)
        {
            _appConfig = appConfig;
            _log = log;
            _workerService = workerService;
        }

        public IEnumerable<IWorker> LoadAll()
        {
            _workerService.ResetAll();

            var plugins = Load();
          
            foreach (var plugin in plugins)
            {
                _workerService.AddOrUpdate(plugin);
                _log.LogInformation($"Load plugin [{plugin.Name}({plugin.Key})]..");
            }
            return plugins;
        }

        private IEnumerable<IWorker> Load()
        {

            List<IWorker> pluginsLibs = new List<IWorker>();

            var pluginsOptions = _appConfig.Operator.plugins;

            //https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/creating-app-with-plugin-support

            if (pluginsOptions != null)
            {
                var throwIfPluginNotFound = _appConfig.Operator.sys.throwIfPluginNotFound.HasValue ? _appConfig.Operator.sys.throwIfPluginNotFound.Value : true;
                foreach (var plg in pluginsOptions)
                {
                    try
                    {
                        if (File.Exists(plg.path))
                        {
                            Assembly pluginAssembly = LoadPlugin(plg.path);
                            var ws = CreateWorkers(pluginAssembly);
                            pluginsLibs.AddRange(ws);
                        }
                        else
                        {
                            throw new Exception($"Plugin [{plg.key}] not found. throwIfPluginNotFound={throwIfPluginNotFound}");
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

        private IEnumerable<IWorker> CreateWorkers(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IWorker).IsAssignableFrom(type))
                {
                    IWorker result = Activator.CreateInstance(type) as IWorker;
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
    }
}