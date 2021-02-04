using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rec = new RmqTasking.Receiver();
            //rec.Start();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    //services.AddHostedService<VideosWatcher>();
                });
    }
}
