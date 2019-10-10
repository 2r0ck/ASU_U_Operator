using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ASU_U_Operator.Model
{
    [Table("Shedulers", Schema = "shell")]
    public class OperatorSheduler
    {
        public int Id { get; set; }
        public string Command { get; set; }

        public string JsonData { get; set; }

        public ShedulerStatus? Status { get; set; }

        public string Info { get; set; }

        public DateTime? ProcessingDate { get; set; }

    }
}
