using ASU_U_Operator.Configuration;
using ASU_U_Operator.Services;
using ASU_U_Operator.Shell;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private readonly IOperatorShell _shell;
        private IEnumerable<IWorker> plugins;
        private CancellationToken mainStoppingToken;
        private CancellationTokenSource shellStoppingTokenSource;

        public CoreHost(IServiceProvider serviceProvider,
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
            _serviceProvider = serviceProvider ?? throw new InvalidProgramException("ServiceProvider not defined");
            //SIG handlers
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            _healthcheck.Error += _healthcheck_Error;
        }

        private ShellTaskCallback _shell_StopPlugin(Guid arg)
        {
            throw new NotImplementedException();
        }

        private ShellTaskCallback _shell_StartPlugin(Guid arg)
        {
            throw new NotImplementedException();
        }

        private void _healthcheck_Error(IWorker worker, Exception exception)
        {
            _logger.LogError(exception.ToString());
            //перезапускаем плагин  
            _logger.LogInformation("Restart plugin...");
            _coreInitializer.StopPlugin(worker);
            ClearMemory(); //очищаем все объекты оставшиеся после плагина

            Thread.Sleep(_appConfig.Operator.sys.restartPluginTimeoutMs??5000);

            _coreInitializer.RunPlugin(worker, mainStoppingToken);
        }

        private void OnStopping()
        {
            //stop shell
            if (!shellStoppingTokenSource.IsCancellationRequested)
            {
                shellStoppingTokenSource.Cancel();
                shellStoppingTokenSource.Dispose();
            }
            _logger.LogInformation("Stoping plugins..");

            if (plugins != null)
            {
                bool success = true;
                foreach (var plugin in plugins)
                {
                    success &= _coreInitializer.StopPlugin(plugin);
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
                mainStoppingToken = stoppingToken;
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
                    _coreInitializer.RunPlugin(plugin, stoppingToken);
                }

                shellStoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                await _shell.Run(shellStoppingTokenSource.Token);
            }
            catch (Exception ex)
            {
                FatalExit(ex);
            }
        }

    

        private  object lockClearMemory = new object();

        public  void ClearMemory()
        {
            lock (lockClearMemory)
            {
                try
                {                    
                    _logger.LogInformation("(1) Total Memory: " + (GC.GetTotalMemory(false) / 1024 / 1024));
                    //1
                    GC.Collect();
                    //2
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    // Collect anything that's just been finalized
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                    _logger.LogInformation("(2) Total Memory: " + (GC.GetTotalMemory(false) / 1024 / 1024));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
        }
    }
}