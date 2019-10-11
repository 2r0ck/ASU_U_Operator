using ASU_U_Operator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASU_U_Operator.Services
{
    public class ShedulerService : IShedulerService
    {
        private readonly OperatorDbContext context;

        public ShedulerService(OperatorDbContext con)
        {
            this.context = con ?? throw new InvalidProgramException("DBContext not defined");
        }
        public IEnumerable<OperatorSheduler> GetNew()
        {
            return context.Shedulers.Where(x => x.Status == null).ToList();                        
        }

        public  IEnumerable<OperatorSheduler> MarkAs(IEnumerable<OperatorSheduler> shedulers, ShedulerStatus status,string info = null)
        {
            var ids = shedulers.Select(x => x.Id).ToList();
            var now = DateTime.Now;
            var db_shedulers = context.Shedulers.Where(x => ids.Contains(x.Id));
            foreach (var shed in db_shedulers)
            {
                if (status == ShedulerStatus.Processing)
                {
                    shed.ProcessingDate = now;
                }

                if (!string.IsNullOrEmpty(info))
                {
                    shed.Info = info;
                }

                shed.Status = status;
            }
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
            return db_shedulers;
        }

        
    }
}
