using ASU_U_Operator.Configuration;
using ASU_U_Operator.Model;
using ASU_U_Operator.Services;
using ASU_U_Operator.Shell.Shedulers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASU_U_Operator.Shell.DataBase
{
    internal class DataBaseShell : IOperatorShell
    {
        private readonly IPreparedAppConfig _config;
        private readonly ILogger<DataBaseShell> _logger;
        private readonly IShedulerService _shedulerService;
        private readonly IServiceProvider _serviceProvider;

        public DataBaseShell(IPreparedAppConfig config, ILogger<DataBaseShell> logger, IShedulerService shedulerService, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new InvalidProgramException("ServiceProvider not defined");
            _config = config ?? throw new InvalidProgramException("CoreInitializer not defined");
            _logger = logger ?? throw new InvalidProgramException("AppLifetime not defined");
            _shedulerService = shedulerService ?? throw new InvalidProgramException("ShedulerService not defined");
        }

        public Task RunShell(CancellationToken stoppingToken)
        {
            //todo: обеспечить отказоустойчивось
            //в отдельной таблице держи все плагины подключенные вручную
            try
            {
                _logger.LogInformation("Starting DataBaseShell");
                var tickMs = _config.Operator.shell.tickTimeoutMs ?? 5000;
                //работа оболочки сихнонная, поэтому все задачи по одному энкземпляру
                var shShed = _serviceProvider.GetServices<ISheduler>();

                Dictionary<string, ISheduler> shellShedulers = new Dictionary<string, ISheduler>();

                foreach (var sheduler in shShed)
                {
                    _logger.LogInformation($"Load command [{sheduler.Command}] to shell");
                    if (shellShedulers.ContainsKey(sheduler.Command))
                    {
                        _logger.LogError($"Shell sheduler [{sheduler.Command}] already exist. Sheduler not loaded.");
                        continue;
                    }
                    shellShedulers.Add(sheduler.Command, sheduler);
                }

                while (true)
                {
                    //todo: BUG-> caching context result
                    var shedulers = _shedulerService.GetNew();
                    if (shedulers.Any())
                    {
                        shedulers = _shedulerService.MarkAs(shedulers, ShedulerStatus.Processing);
                        foreach (var shed in shedulers)
                        {
                            try
                            {
                                if ("help".Equals(shed.Command?.ToLower()))
                                {
                                    var help = "Shell Commands: \n";
                                    help += string.Join(Environment.NewLine, shShed.Select(x => x.Desc));
                                    _shedulerService.MarkAs(new[] { shed }, ShedulerStatus.Done,help);
                                    continue;
                                }

                                if (shellShedulers.ContainsKey(shed.Command?.ToLower() ?? ""))
                                {                                    
                                    var handler = shellShedulers[shed.Command?.ToLower() ?? ""];
                                    _shedulerService.MarkAs(shedulers, ShedulerStatus.Starting);
                                   var result =  handler.Go(shed.JsonData, stoppingToken);
                                    _shedulerService.MarkAs(shedulers, result.Status,result.Message);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"Unknown command: [{shed.Command}]");
                                }

                            }
                            catch (Exception ex)
                            {
                                _shedulerService.MarkAs(new[] { shed }, ShedulerStatus.Error,ex.ToString());
                            }
                        }
                    }

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