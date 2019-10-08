using ASU_U_Operator.Configuration;
using ASU_U_Operator.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    internal class CoreHost : BackgroundService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<CoreHost> _logger;
        private readonly ICoreInitializer _coreInitializer;
        private readonly IPreparedAppConfig _appConfig;
        private readonly IWorkerService _workerService;
        private readonly IHealthcheck _healthcheck;
        private IEnumerable<IWorker> plugins;

        public CoreHost(IServiceProvider serviceProvider,
            IHostApplicationLifetime appLifetime,
            ILogger<CoreHost> logger,
            ICoreInitializer coreInitializer,
            IPreparedAppConfig appConfig,
            IWorkerService workerService,
            IHealthcheck healthcheck)
        {
            _appLifetime = appLifetime ?? throw new InvalidProgramException("AppLifetime not defined");
            _logger = logger ?? throw new InvalidProgramException("AppLifetime not defined");
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
            _appConfig = appConfig ?? throw new InvalidProgramException("CoreInitializer not defined");
            _workerService = workerService ?? throw new InvalidProgramException("WorkerService not defined");
            _healthcheck = healthcheck;
            _serviceProvider = serviceProvider ?? throw new InvalidProgramException("ServiceProvider not defined");
            //SIG handlers
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            _healthcheck.Error += _healthcheck_Error;
        }

        private void _healthcheck_Error(IWorker worker, Exception exception)
        {
            _logger.LogError(exception.ToString());
            //перезапускаем плагин если ошибка - загружать снова не требуется

            StopPlugin(worker);

            //todo restart
            
        }

        private void OnStopping()
        {
            _logger.LogInformation("Stoping plugins..");

            if (plugins != null)
            {
                bool success = true;
                foreach (var plugin in plugins)
                {
                    success &= StopPlugin(plugin);
                }
                if (!success)
                {
                    Environment.ExitCode = 1;
                }
                _logger.LogInformation("All plugin stoped.");
            }
        }

        private void OnStopped()
        {
            _logger.LogInformation("Core stopped.");
        }

        private void OnStarted()
        {
            _logger.LogInformation("All plugin started.");
        }

        protected void FatalExit(Exception ex)
        {
            _logger.LogError(ex.ToString());
            _logger.LogWarning($"FatalExit(1)");
            Environment.ExitCode = 1;
            _appLifetime.StopApplication();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!_appConfig.Validate())
                {
                    throw new ApplicationException("Config file has errors");
                }

                plugins = _coreInitializer.LoadAll();

                if (plugins == null || plugins.Count() == 0)
                {
                    throw new Exception($"plugins not found");
                }

                foreach (var plugin in plugins)
                {
                    PreparePlugin( plugin);
                }

                await Task.WhenAll(plugins.Select(x => x.Start()).ToList());
            }
            catch (Exception ex)
            {
                FatalExit(ex);
            }
        }

        private void PreparePlugin( IWorker plugin)
        {
            var throwIfInitError = _appConfig.Operator.sys.throwIfInitError ?? false;
            var init = InitPlugin(plugin);
            if (!init.IsCompletedSuccessfully)
            {
                Exception ex = init.Exception ?? new Exception($"plugin [{plugin.Name}({plugin.Key})] initialize with errors");
                _logger.LogError(ex.ToString());
                if (throwIfInitError)
                {
                    throw ex;
                }
            }
            else
            {
                _workerService.MarkInit(plugin.Key, true);
                _logger.LogInformation($"plugin [{plugin.Name}({plugin.Key})] successfully initialize.");
                //после инициализации плагин должен положительно отвечать на healthcheck
                if (_appConfig.Operator.sys.enableHealthcheck ?? false)
                {
                    //healthcheck
                    _healthcheck.RunNew(plugin);
                }
            }
        }

        private Task InitPlugin(IWorker plugin)
        {
            _logger.LogInformation($"Init plugin {plugin.Name}({plugin.Key})");
            var timeout = _appConfig.Operator.sys.pluginShutdownTimeoutMs ?? 5000;

            var init = plugin.Init();

            if (Task.WhenAny(init, Task.Delay(timeout)).Result != init)
            {
                Task.FromException(new Exception($"Init plugin timeout expired! Plugin:[{plugin.Name}({plugin.Key})]"));
            }
            return init;
        }

        
        private bool StopPlugin(IWorker plugin)
        {
            try
            {
                _logger.LogInformation($"Stop plugin {plugin.Name}({plugin.Key})");

                var timeout = _appConfig.Operator.sys.pluginShutdownTimeoutMs ?? 5000;

                _healthcheck.Stop(plugin.Key);

                var stop = plugin.Stop();
                if (Task.WhenAny(stop, Task.Delay(timeout)).Result != stop)
                {
                    throw new Exception($"Stoping timeout expired! Plugin:[{plugin.Name}({plugin.Key})]");
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
                _logger.LogError(ex.ToString());
                return false;
            }
        }
    }
}