using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Receiver
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }



        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<Receiver>();

            services.AddSingleton<IDistributionChannel, DistributionChannel>();

            //services.AddSingleton<ITaskDistributor, TaskDistributor>();
            services.AddHostedService<TaskDistributor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
