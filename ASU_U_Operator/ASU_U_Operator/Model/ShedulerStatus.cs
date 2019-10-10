using System;
using System.Collections.Generic;
using System.Text;

namespace ASU_U_Operator.Model
{
    public enum ShedulerStatus:byte
    {
        Processing=0,
        Starting=1,
        Done=2,
        Error=3
    }
}
