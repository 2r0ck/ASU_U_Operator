using ASU_U_Operator.Configuration;
using ASU_U_Operator.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASU_U_Operator.Shell.DataBase
{
    internal class DataBaseShell : IOperatorShell
    {
        private readonly IPreparedAppConfig _config;
        private readonly ILogger<DataBaseShell> _logger;
        private readonly ICoreInitializer _coreInitializer;

        public DataBaseShell(IPreparedAppConfig config,ILogger<DataBaseShell> logger,  ICoreInitializer coreInitializer)
        {
            _config = config ?? throw new InvalidProgramException("CoreInitializer not defined");
            _logger = logger ?? throw new InvalidProgramException("AppLifetime not defined");
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
        }

     

        public Task Run(CancellationToken stoppingToken)
        {
            
            //todo: обеспечить отказоустойчивось
            //в отдельной таблице держи все плагины подключенные вручную
            try
            {
                _logger.LogInformation("Starting DataBaseShell");
                while (true)
                {

                    var tickMs = _config.Operator.shell.tickTimeoutMs ?? 5000;
                    Thread.Sleep(tickMs);
                    stoppingToken.ThrowIfCancellationRequested();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Stop DataBaseShell");
                if (ex is OperationCanceledException)
                {
                    return Task.CompletedTask;
                }
                _logger.LogError(ex.ToString());
                return Task.FromException(ex);
            } 
        }
    }
}
