using ASU_U_Operator.Configuration;
using ASU_U_Operator.Core;
using ASU_U_Operator.Model;
using ASU_U_Operator.Shell.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace ASU_U_Operator.Shell.Shedulers
{
    public class AttachNewPluginSheduler : ShedulerBase<ShedulerExecData>
    {
        private readonly ICoreInitializer _coreInitializer;
        private readonly ILogger<AttachNewPluginSheduler> logger;

        public override string Command => "attachnew";

        public override string Desc => "RefreshPlugin : Refresh all plugin by appsettings.josn. \n JSON Data is null";

        public AttachNewPluginSheduler(ICoreInitializer coreInitializer, ILogger<AttachNewPluginSheduler> log)
        {
            _coreInitializer = coreInitializer ?? throw new InvalidProgramException("CoreInitializer not defined");
            logger = log ?? throw new InvalidProgramException("Logger<AttachNewPluginSheduler> not defined");
        }

        public override ShedulerEventArgs Go(string json, CancellationToken stoppingToken)
        {
            logger.LogInformation("Shell: AttachNew run");
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var conf = builder.Build();
            var new_operator = new OperatorSection();
            conf.GetSection("operator").Bind(new_operator);
            //загружаем новые
            var guids = _coreInitializer.AttachByNewOptions(new_operator);

            var res = true;

            if(guids==null || !guids.Any())
            {
                return new ShedulerEventArgs()
                {
                    Status = ShedulerStatus.Done,
                    Message = "AttachNewPluginSheduler: new plugins not found"
                };
            }
            //запускаем
            foreach (var pluginKey in guids)
            {
                res &= _coreInitializer.RunPlugin(pluginKey, stoppingToken);
            }
            if (res)
            {
                return new ShedulerEventArgs()
                {
                    Status = ShedulerStatus.Done
                };
            }
            return new ShedulerEventArgs()
            {
                Status = ShedulerStatus.Error,
                Message = "AttachNewPluginSheduler error occurred. See log for detail"
            };
        }
    }
}