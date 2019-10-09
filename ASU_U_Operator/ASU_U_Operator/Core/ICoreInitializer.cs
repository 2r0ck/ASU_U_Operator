using System.Collections.Generic;
using System.Threading;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    public interface ICoreInitializer
    {
        IEnumerable<IWorker> LoadAll();

        void RunPlugin(IWorker plugin, CancellationToken stoppingToken);

        bool StopPlugin(IWorker plugin);
    }
}