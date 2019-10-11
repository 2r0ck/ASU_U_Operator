using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ASU_U_Operator.Shell.Shedulers
{
    public interface ISheduler
    {
        string Command { get;  }
        string Desc { get; }

        ShedulerEventArgs Go(string json, CancellationToken stoppingToken);       
    }
}
