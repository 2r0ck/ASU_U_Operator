using Newtonsoft.Json;
using System;
using System.Threading;

namespace ASU_U_Operator.Shell.Shedulers
{
    public abstract class ShedulerBase<T> : ISheduler
    {
        public abstract string Command { get;  }

        public abstract string Desc { get; }

        public abstract ShedulerEventArgs Go(string json, CancellationToken stoppingToken);

        public virtual T GetData(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Json data is empty");
            }

            var data =  JsonConvert.DeserializeObject<T>(json);

            if(data == null )
            {
                throw new Exception("Json data is wrong");
            }

            return data;
        }
    }
}