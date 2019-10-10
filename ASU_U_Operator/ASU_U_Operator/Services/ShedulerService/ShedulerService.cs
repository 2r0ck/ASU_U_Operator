using ASU_U_Operator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASU_U_Operator.Services
{
    public class ShedulerService
    {
        private readonly OperatorDbContext context;

        public ShedulerService(OperatorDbContext con)
        {
            this.context = con ?? throw new InvalidProgramException("DBContext not defined");
        }
        public IEnumerable<OperatorSheduler> GetNew()
        {
            return context.Shedulers.Where(x => x.Status == null);                        
        }

        public IEnumerable<OperatorSheduler> MarkAsProccedd(IEnumerable<OperatorSheduler> shedulers)
        {
            var ids = shedulers.Select(x => x.Id).ToList();
            //var shedulers = context.Shedulers.Where(x => x.Status == null).ToList();
            return null;
        }
    }
}
