using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASU_U_Operator.Configuration
{
    public class PreparedAppConfig : IPreparedAppConfig
    {
        public OperatorSection Operator { get; }
        private readonly ILogger<PreparedAppConfig> log;

        public PreparedAppConfig(IConfiguration configuration,ILogger<PreparedAppConfig> log)
        {
            Operator = new OperatorSection();
            configuration.GetSection("operator").Bind(Operator);
            this.log = log;
        }

        public bool Validate()
        {
            if (Operator==null)
            {
                log.LogError("Config section [operator] not declared!");
            }


            if (Operator.Validate())
            {
                return true;
            }
            log.LogError("Config file has error!");

            foreach (var msg in Operator.ErrorMessages)
            {
                log.LogError(msg);
            }
            return false;
        }
    }
}
