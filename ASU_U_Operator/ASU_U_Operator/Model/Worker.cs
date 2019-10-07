using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ASU_U_Operator.Model
{
    [Table("Workers", Schema = "operator")]
    public  class Worker
    {
        [Key]
        public Guid Key { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Version { get; set; }

        public string Path { get; set; }

        public DateTime LastInitDate { get; set; }

        public bool ThrowIfInitError { get; set; }

        public bool ThrowIfHealthcheckError { get; set; }

        public int ShutdownTimeoutMs { get; set; }

    }
}
