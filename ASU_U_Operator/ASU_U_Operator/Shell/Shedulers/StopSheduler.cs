using ASU_U_Operator.Core;
using ASU_U_Operator.Model;
using ASU_U_Operator.Services;
using ASU_U_Operator.Shell.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ASU_U_Operator.Shell.Shedulers
{
    public class StopSheduler : ShedulerBase<ShedulerExecData>
    {
        private readonly ICoreInitializer _coreInitializer;
        private readonly ILogger<StopSheduler> logger;

        public override string Command => "stop";

        public override string Desc => "stop : Stop plugin by GUID. \n JSON Data Sample: {\"key\":\"6354780B-0E8C-4C39-A727-02A97D6E956C\"}";

        public StopSheduler(ICoreInitializer coreInitializer,   ILogger<StopSheduler> log)
        {
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
            logger = log ?? throw new InvalidProgramException("Logger<StopSheduler> not defined");
        }
        public override ShedulerEventArgs Go(string json, CancellationToken stoppingToken)
        {
            logger.LogInformation("Shell: StopSheduler run");
            var data = GetData(json);
            var res = _coreInitializer.StopPlugin(data.Key);
            if (res)
            {
                return new ShedulerEventArgs()
                {
                    Status = ShedulerStatus.Done
                };
            }
            return new ShedulerEventArgs()
            {
                Status = ShedulerStatus.Error,
                Message = "Plugin stop error occurred. See log for detail"
            };

        }
    }
}
