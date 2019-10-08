using System;
using System.Threading.Tasks;
using WorkerBase;

namespace ASU_U_Operator.Services
{
    public interface IHealthcheck
    {
        void RunNew(IWorker worker);

        event Action<IWorker, Exception> Error;

        void Stop(Guid key);
    }
}