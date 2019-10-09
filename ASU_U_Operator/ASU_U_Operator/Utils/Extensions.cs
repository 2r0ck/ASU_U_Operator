using System;
using System.Collections.Generic;
using System.Text;
using WorkerBase;

namespace ASU_U_Operator 
{
    public static class Extensions
    {

        public static string Info(this IWorker worker)
        {
            if (worker == null)
            {
                return "#empty_iworker";
            }

            return $"[{worker.Name}({worker.Key})]";
        }
    }
}
