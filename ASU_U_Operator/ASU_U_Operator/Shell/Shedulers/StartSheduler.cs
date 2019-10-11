using ASU_U_Operator.Core;
using ASU_U_Operator.Model;
using ASU_U_Operator.Shell.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ASU_U_Operator.Shell.Shedulers
{
    public class StartSheduler : ShedulerBase<ShedulerExecData>
    {
        private readonly ICoreInitializer _coreInitializer;

        public override string Command => "start";

        public override string Desc => "start : Start plugin by GUID. \n JSON Data Sample: {\"key\":\"6354780B-0E8C-4C39-A727-02A97D6E956C\"}";

        public StartSheduler(ICoreInitializer coreInitializer)
        {
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
        }
        public override ShedulerEventArgs Go(string json, CancellationToken stoppingToken)
        {
            var data = GetData(json);
            var res =  _coreInitializer.RunPlugin(data.Key, stoppingToken);
            if (res)
            {
                return new ShedulerEventArgs() {
                    Status = ShedulerStatus.Done
                };
            }
            return new ShedulerEventArgs()
            {
                Status = ShedulerStatus.Error,
                Message = "Plugin run error occurred. See log for detail"
            };

        }
    }
}
