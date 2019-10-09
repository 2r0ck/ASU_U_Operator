using ASU_U_Operator.Configuration;
using ASU_U_Operator.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASU_U_Operator.Shell.DataBase
{
    class DataBaseShell : IOperatorShell
    {
        private readonly IPreparedAppConfig _config;
        

        public DataBaseShell(IPreparedAppConfig config )
        {
            _config = config;           
        }

        public event Func<Guid, ShellTaskCallback> StartPlugin;
        public event Func<Guid, ShellTaskCallback> StopPlugin;

        public Task Run(CancellationToken stoppingToken)
        {
            //todo: обеспечить отказоустойчивось
            while (true)
            {
                var tickMs = _config.Operator.shell.tickTimeoutMs?? 5000;
                Thread.Sleep(tickMs);
            }
        }
    }
}
