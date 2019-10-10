using System;
using System.Collections.Generic;
using System.Threading;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    public interface ICoreInitializer
    {
        IEnumerable<Guid> LoadAll();

        bool RunPlugin(Guid pluginKey, CancellationToken stoppingToken);

        bool StopPlugin(Guid pluginKey);
    }
}