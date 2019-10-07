using ASU_U_Operator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ASU_U_Operator.Core
{
    public class CoreInitializer : ICoreInitializer
    {
        readonly OperatorDbContext context;

        public CoreInitializer(OperatorDbContext context)
        {
            this.context = context;
        }

        public void Init()
        {
            try
            {
                var dbWorkers = context.Workers.ToList();

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
                //todo: изменить подгрузку модулей брать пути из файла?
                var files = Directory.GetFiles(path, "*.dll");

                //https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/creating-app-with-plugin-support

            }
            catch (Exception ex)
            {

            }
          
        }
    }
}
