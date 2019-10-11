using System.Collections.Generic;
using ASU_U_Operator.Model;

namespace ASU_U_Operator.Services
{
    public interface IShedulerService
    {
        IEnumerable<OperatorSheduler> GetNew();
        IEnumerable<OperatorSheduler> MarkAs(IEnumerable<OperatorSheduler> shedulers, ShedulerStatus status, string info = null);
    }
}