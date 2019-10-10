using System;
using System.Collections.Generic;
using System.Text;

namespace ASU_U_Operator.Shell
{
    public class ShedulerCommand 
    {
        public string  Command { get; set; }
        public string RawData{ get; set; }

        public T GetData<T>() where T: class 
        {
            return null;
        }
    }


     
}
