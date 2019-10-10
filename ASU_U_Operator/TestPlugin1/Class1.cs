﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WorkerBase;

namespace TestPlugin1
{
    public class Worker : IPluginWorker
    {
        public Guid Key => Guid.Parse("6354780B-0E8C-4C39-A727-02A97D6E956C");

        public string Name => "Worker1";

        public string Description => "test plugin";

        public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        

        private Guid CurrnetGuid;

        //упадет после 5 сек после инициализации
        public Task<bool> Healthcheck() { return Task.FromResult((DateTime.Now - initTime).Seconds < 5); }

        private DateTime initTime;

        public async Task Start(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
           {
               //todo: test exception
               while (true)
               {
                   Console.WriteLine($"*****Plugin :{Name}, action Start*****{CurrnetGuid}");
                   Thread.Sleep(1000);
                   stoppingToken.ThrowIfCancellationRequested();
               }               
           });
        }

        public async Task  Stop()
        {
            await Task.Run(() =>
            {
              
                Console.WriteLine($"Plugin :{Name}, action Stop");
            });
        }

        public Task<bool> Init()
        {
            //if((DateTime.Now - initTime).Seconds < 15)
            //{
            //    throw new Exception("TestInitException");
            //}

            Console.WriteLine($"Plugin :{Name}, action Init");
            initTime = DateTime.Now;
            CurrnetGuid = Guid.NewGuid();
            return Task.FromResult(true);           
        }
    }
}
