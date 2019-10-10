using System;
using WorkerBase;

namespace ASU_U_Operator.Services
{
    public interface IWorkerService
    {
        void AddOrUpdate(IPluginWorker plugin);
        void ResetAll();
        void MarkInit(Guid key, bool init);
    }
}