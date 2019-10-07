
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
        
         bool ThrowIfInitError { get;  }
        
         bool ThrowIfHealthcheckError { get; }
        
         int? ShutdownTimeoutMs { get;  }

        bool Init();

        Task Start();

        Task Stop();

    }
}
