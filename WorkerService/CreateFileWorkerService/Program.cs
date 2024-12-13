using CreateFileWorkerService.Models;
using CreateFileWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace CreateFileWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri
                (builder.Configuration.GetConnectionString("RabbitMQ")!)
            });
            builder.Services.AddDbContext<AdventureWorks2022Context>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            });
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<RabbitMQClientService>();
            var host = builder.Build();
            host.Run();
        }
    }
}