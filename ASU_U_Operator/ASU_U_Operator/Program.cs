using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using ASU_U_Operator.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ASU_U_Operator.Model;
using Microsoft.EntityFrameworkCore;
using ASU_U_Operator.Configuration;
using ASU_U_Operator.Services;
using ASU_U_Operator.Shell.DataBase;
using ASU_U_Operator.Shell;

namespace ASU_U_Operator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host =
                new HostBuilder()
                 .ConfigureAppConfiguration((context, builder) =>
                 {
                     builder.AddJsonFile("appsettings.json");                     
                 })
                .ConfigureServices((hBuilder,sc) => {
                    sc.AddScoped<IPreparedAppConfig, PreparedAppConfig>();
                    sc.AddLogging(conf => {
                        conf.AddFilter("ASU_U_Operator", LogLevel.Debug);

                        conf.AddConsole();
                    });                    
                    sc.AddDbContext<OperatorDbContext>(opt => opt.UseSqlServer(hBuilder.Configuration["operator:connectionString:operatorDb"]));
                    sc.AddScoped<ICoreInitializer, CoreInitializer>();
                    sc.AddScoped<IWorkerService, WorkerService>();
                    sc.AddScoped<IHealthcheck, Healthcheck>();
                    sc.AddScoped<IOperatorShell, DataBaseShell>();
                    sc.AddHostedService<CoreHost>();
                    
                }).Build();

            await host.RunAsync();
        }
    }
}
