using System;
using System.Reflection;
using System.Threading.Tasks;
using WorkerBase;

namespace TestPlugin1
{
    public class Worker : IWorker
    {
        public Guid Key => Guid.Parse("6354780B-0E8C-4C39-A727-02A97D6E956C");

        public string Name => "Worker1";

        public string Description => "test plugin";

        public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        private bool load = false;

        public Task<bool> Healthcheck() => Task.FromResult(this.load);

      

        public async Task Start()
        {
            await Task.Run(() =>
           {
               //todo: test exception
               Console.WriteLine($"Plugin :{Name}, action Start");
           });
        }

        public async Task  Stop()
        {
            await Task.Run(() =>
            {
                load = false;
                Console.WriteLine($"Plugin :{Name}, action Stop");
            });
        }

        public Task<bool> Init()
        {
            Console.WriteLine($"Plugin :{Name}, action Init");
            load = true;
            return Task.FromResult(true);           
        }
    }
}
