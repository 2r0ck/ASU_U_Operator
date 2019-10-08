
using System;
using System.Threading.Tasks;

namespace WorkerBase
{
    public interface IWorker
    {       
         Guid Key { get; }
        
         string Name { get; }
         string Description { get;  }
        
         string Version { get;  }       
 

        Task<bool>  Init();

        Task Start();

        Task Stop();

        Task<bool> Healthcheck();
    }
}
