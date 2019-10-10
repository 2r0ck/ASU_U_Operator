using System;
using System.Threading.Tasks;
using WorkerBase;

namespace ASU_U_Operator.Services
{
    public interface IHealthcheck
    {
        void RunNew(IPluginWorker worker);

        event Action<IPluginWorker, Exception> Error;

        void Stop(Guid key);
    }
}