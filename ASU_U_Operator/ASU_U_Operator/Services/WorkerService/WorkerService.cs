using ASU_U_Operator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkerBase;

namespace ASU_U_Operator.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly OperatorDbContext context;

        public WorkerService(OperatorDbContext con)
        {
            this.context = con ?? throw new InvalidProgramException("DBContext not defined"); 
        }


        public void ResetAll()
        {
            var dbWorkers = context.Workers.ToList();

            foreach (var worker in dbWorkers)
            {
                worker.LastLoadDate = DateTime.Now;
                worker.Loaded = false;
                worker.Init = false;
            }
            Save();
        }

        public void AddOrUpdate(IWorker plugin)
        {
            var dbPlugin = context.Workers.Where(x => x.Key == plugin.Key).FirstOrDefault();
            if (dbPlugin == null)
            {
                dbPlugin = new Worker();
                dbPlugin.Key = plugin.Key;
                context.Workers.Add(dbPlugin);
            }
            var now = DateTime.Now;
            dbPlugin.LastLoadDate = now;
            dbPlugin.Name = plugin.Name;
            dbPlugin.Path = plugin.GetType().Assembly.Location;
            dbPlugin.Version = plugin.Version;
            dbPlugin.Description = plugin.Description;
            dbPlugin.Loaded = true;
            Save();
        }


        public void MarkInit(Guid key,bool init)
        {
            var pluginDbData = context.Workers.FirstOrDefault(x => x.Key == key);
            if (pluginDbData == null)
            {
                throw new InvalidProgramException("MarkAsInit: plugin not found!");
            }
            pluginDbData.Init = init;
            if (init)
            {
                pluginDbData.LastInitDate = DateTime.Now;
            }
            Save();
        }

        private void Save()
        {
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
        }
    }
}
