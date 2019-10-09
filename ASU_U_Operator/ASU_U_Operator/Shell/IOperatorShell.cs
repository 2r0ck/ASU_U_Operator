using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASU_U_Operator.Shell
{
    public interface IOperatorShell
    {
        Task Run(CancellationToken stoppingToken);

        event Func<Guid, ShellTaskCallback> StartPlugin;

        event Func<Guid, ShellTaskCallback> StopPlugin; 
}
}
