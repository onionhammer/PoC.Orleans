using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using static Orleans.Hosting.ClientStreamExtensions;

namespace PoC.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Add Orleans client
            services.AddSingleton(CreateClusterClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        /// <summary>
        /// Create a cluster client
        /// </summary>
        public static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetService<ILogger<Startup>>();

            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = "PowerCommandCloud";
                    options.ClusterId = "dev"; // TODO Set cluster id
                })
                .AddSimpleMessageStreamProvider("SMSProvider", options =>
                {
                    options.FireAndForgetDelivery    = true;
                    options.OptimizeForImmutableData = true;
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            client.Connect(RetryFilter).GetAwaiter().GetResult();
            return client;

            async Task<bool> RetryFilter(Exception exception)
            {
                log?.LogWarning("Exception while attempting to connect to Orleans cluster: {Exception}", exception);
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
        }
    }
}
