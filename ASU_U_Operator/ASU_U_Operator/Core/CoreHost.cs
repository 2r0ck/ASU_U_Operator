using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private IEnumerable<IWorker> plugins;

        public CoreHost(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, ILogger<CoreHost> logger, ICoreInitializer coreInitializer)
        {
            _logger = logger;
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
            _appLifetime = appLifetime ?? throw new InvalidProgramException("AppLifetime not defined");
            _serviceProvider = serviceProvider ?? throw new InvalidProgramException("ServiceProvider not defined");
            //SIG handlers
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        private void OnStopping()
        {
            _logger.LogInformation("Core stopping..");
        }

        private void OnStopped()
        {
            _logger.LogInformation("Core stopped..");
        }

        private void OnStarted()
        {
            _logger.LogInformation("All services started.");
            plugins = _coreInitializer.Init();

            if(plugins==null && plugins.Count() == 0)
            {
                FatalExit(new Exception($"plugins not found"));
            }

            foreach (var plugin in plugins)
            {
                bool success = false;
                Exception err = null;
                try
                {
                    success = plugin.Init();                  
                }
                catch (Exception ex)
                {
                    err = ex;
                }
                
                if(!success && plugin.ThrowIfInitError)
                {
                    FatalExit(err ?? new Exception($"plugin [{plugin.Name}] return false"));
                }
            }
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
                await Task.WhenAll(plugins.Select(x=>x.Start()).ToList());          
            }
            catch (Exception ex)
            {
                FatalExit(ex);               
            }
        }
    }
}
