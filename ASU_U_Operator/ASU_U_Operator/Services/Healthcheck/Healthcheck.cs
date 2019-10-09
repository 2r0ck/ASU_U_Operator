using ASU_U_Operator.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WorkerBase;

namespace ASU_U_Operator.Services
{
    public class Healthcheck : IHealthcheck
    {
        private readonly IPreparedAppConfig _appConfig;
        private readonly ILogger<Healthcheck> _logger;

        private int HealthcheckInterval => _appConfig.Operator.sys.healthcheckIntervalMs ?? 60000;

        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> tokens;

        public event Action<IWorker, Exception> Error;

        public Healthcheck(IPreparedAppConfig appConfig, ILogger<Healthcheck> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
            tokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        }

        public void RunNew(IWorker worker)
        {
            CancellationTokenSource csource = new CancellationTokenSource();
            var token = csource.Token;
            if (tokens.ContainsKey(worker.Key))
            {
                throw new Exception($"Worker key {worker.Info()} already exist in Healthcheck");
            }

            tokens.TryAdd(worker.Key, csource);

            Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation($"Healthcheck: {worker.Info()} start.");

                    while (!token.IsCancellationRequested)
                    {
                        _logger.LogDebug($"Healthcheck: {worker.Info()} tick ({HealthcheckInterval})");

                        var res = await worker.Healthcheck();
                        if (!res)
                        {
                            throw new Exception($"Healthcheck return false. Plugin {worker.Info()}");
                        }
                        Thread.Sleep(HealthcheckInterval);
                    }
                }catch (Exception ex)
                {
                    Error?.Invoke(worker, ex);
                }
            });
        }

       

        public void Stop(Guid key)
        {
            CancellationTokenSource tokenSource = null;
            if (tokens.TryRemove(key, out tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
    }
}