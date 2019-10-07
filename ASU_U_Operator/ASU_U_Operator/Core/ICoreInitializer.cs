using System.Collections.Generic;
using WorkerBase;

namespace ASU_U_Operator.Core
{
    public interface ICoreInitializer
    {
        IEnumerable<IWorker>  Init();
    }
}