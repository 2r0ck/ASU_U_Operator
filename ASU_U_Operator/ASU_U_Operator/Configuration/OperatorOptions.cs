using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ASU_U_Operator.Configuration
{
 
    public class OperatorSection :CfgSectionBase
    {
        [Required]
        public SysSection sys { get; set; }       
     

        public ShellSection shell { get; set; }

        public PluginSection[] plugins { get; set; }

        public override bool Validate()
        {
            return ValidateChild(shell) &&  ValidateChild(sys)  && base.Validate();
        }
       
    }
    public class ShellSection : CfgSectionBase
    {
         public int? tickTimeoutMs { get; set; }
    }

    public class SysSection : CfgSectionBase
    {
        public int healthchek_tick_ms { get; set; }
        public int default_worker_shutdown_ms { get; set; }

        public bool? throwIfPluginNotFound { get; set; }

        public bool? throwIfInitError { get; set; }
        public bool? throwIfHealthcheckError { get; set; }
        public int? pluginShutdownTimeoutMs { get; set; }
        public bool? enableHealthcheck { get; set; }
        public int? healthcheckIntervalMs { get; set; }

        public int? restartPluginTimeoutMs { get; set; }
    }

    public class ConnectionStringSection : CfgSectionBase
    {
        public string operatorDb { get; set; }
    }

    public class PluginSection : CfgSectionBase
    {
        public string header { get; set; }
        public string path { get; set; }
    }


}
