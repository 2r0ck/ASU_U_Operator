using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASU_U_Operator.Model
{
    public class OperatorDbContext : DbContext
    {
        public OperatorDbContext(DbContextOptions options) :base(options)
        {

        }
        public DbSet<Worker> Workers { get; set; }

        public DbSet<OperatorSheduler> Shedulers { get; set; }

        
    }
}
