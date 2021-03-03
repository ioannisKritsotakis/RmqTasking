using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var appConfiguration = BuildConfiguration(args);
            Log.Logger = BuildLogger(appConfiguration);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IDistributionChannel, DistributionChannel>();
                    services.AddHostedService<HeartbeatService>();
                    services.AddHostedService<Receiver>();
                    services.AddHostedService<TaskDistributor>();
                });


        private static ILogger BuildLogger(IConfiguration appConfiguration)
        {
            return new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithMemoryUsage()
                .ReadFrom.Configuration(appConfiguration, "Serilog")
                .CreateLogger();
        }

        private static IConfigurationRoot BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddCommandLine(args)
                .Build();
        }

    }
}
