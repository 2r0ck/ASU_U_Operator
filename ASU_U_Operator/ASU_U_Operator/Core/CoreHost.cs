using ASU_U_Operator.Configuration;
using ASU_U_Operator.Services;
using ASU_U_Operator.Shell;
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

        private readonly ILogger<CoreHost> _logger;
        private readonly ICoreInitializer _coreInitializer;
        private readonly IPreparedAppConfig _appConfig;
        private readonly IWorkerService _workerService;
        private readonly IHealthcheck _healthcheck;
        private readonly IOperatorShell _shell;

        private CancellationToken mainStoppingToken;
        private CancellationTokenSource shellStoppingTokenSource;

        private object lockClearMemory = new object();
        private IEnumerable<Guid> pluginsKeys;

        public CoreHost(
            IHostApplicationLifetime appLifetime,
            ILogger<CoreHost> logger,
            ICoreInitializer coreInitializer,
            IPreparedAppConfig appConfig,
            IWorkerService workerService,
            IHealthcheck healthcheck,
            IOperatorShell shell)
        {
            _appLifetime = appLifetime ?? throw new InvalidProgramException("AppLifetime not defined");
            _logger = logger ?? throw new InvalidProgramException("AppLifetime not defined");
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
            _appConfig = appConfig ?? throw new InvalidProgramException("CoreInitializer not defined");
            _workerService = workerService ?? throw new InvalidProgramException("WorkerService not defined");
            _healthcheck = healthcheck ?? throw new InvalidProgramException("Healthcheck not defined");
            _shell = shell ?? throw new InvalidProgramException("OperatorShell not defined");

            //SIG handlers
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            _healthcheck.Error += _healthcheck_Error;
        }

        private void _healthcheck_Error(IPluginWorker worker, Exception exception)
        {
            _logger.LogError(exception.ToString());
            //перезапускаем плагин
            _logger.LogInformation("Restart plugin...");
            _coreInitializer.StopPlugin(worker.Key);
            ClearMemory(); //очищаем все объекты оставшиеся после плагина

            Thread.Sleep(_appConfig.Operator.sys.restartPluginTimeoutMs ?? 5000);
            _coreInitializer.RunPlugin(worker.Key, mainStoppingToken);
        }

        private void OnStopping()
        {
            //stop shell
            if (shellStoppingTokenSource!=null && !shellStoppingTokenSource.IsCancellationRequested)
            {
                shellStoppingTokenSource.Cancel();
                shellStoppingTokenSource.Dispose();
            }
            _logger.LogInformation("Stopping plugins..");

            if (pluginsKeys != null)
            {
                bool success = true;
                foreach (var pluginKey in pluginsKeys)
                {
                    success &= _coreInitializer.StopPlugin(pluginKey);
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
                _logger.LogInformation("Core host exec..");

                mainStoppingToken = stoppingToken;
                if (!_appConfig.Validate())
                {
                    throw new ApplicationException("Config file has errors");
                }

                pluginsKeys = _coreInitializer.LoadAll();

                if (pluginsKeys == null || !pluginsKeys.Any())
                {
                    throw new Exception($"plugins not found");
                }

                foreach (var pluginKey in pluginsKeys)
                {
                    _coreInitializer.RunPlugin(pluginKey, stoppingToken);
                }

                shellStoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                await _shell.RunShell(shellStoppingTokenSource.Token);
            }
            catch (Exception ex)
            {
                FatalExit(ex);
            }
        }

        public void ClearMemory()
        {
            lock (lockClearMemory)
            {
                try
                {
                    _logger.LogDebug("(1) Total Memory: " + (GC.GetTotalMemory(false) / 1024 / 1024));
                    //1
                    GC.Collect();
                    //2
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    // Collect anything that's just been finalized
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                    _logger.LogDebug("(2) Total Memory: " + (GC.GetTotalMemory(false) / 1024 / 1024));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
        }
    }
}