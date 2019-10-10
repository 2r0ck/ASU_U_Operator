using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASU_U_Operator.Shell
{
    public interface IOperatorShell
    {
        Task RunShell(CancellationToken stoppingToken);

       
}
}
