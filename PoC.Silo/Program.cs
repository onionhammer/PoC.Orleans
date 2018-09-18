using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using PoC.Grains.Implementation;

namespace PoC.Silo
{
    class Program : IHostedService
    {
        readonly ISiloHost silo;

        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices((HostBuilderContext, services) =>
                {
                    services.AddScoped<IHostedService, Program>();
                });

            await hostBuilder.RunConsoleAsync();
        }

        public Program()
        {
            silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure((ClusterOptions options) =>
                {
                    options.ServiceId = "PoCService";
                    options.ClusterId = "dev"; // TODO Set cluster id
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .Configure((ProcessExitHandlingOptions options) => options.FastKillOnProcessExit = false)
                .ConfigureLogging(logging =>
                {
#if DEBUG
                    logging.SetMinimumLevel(LogLevel.Information);
#else
                    logging.SetMinimumLevel(LogLevel.Error);
#endif
                    logging.AddConsole();
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences()
                )
                /* Configure in-memory storage */
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryGrainStorage("GrainStore")
                .UseInMemoryReminderService()
                ///////
                .AddSimpleMessageStreamProvider("SMSProvider", options =>
                {
                    options.FireAndForgetDelivery    = true;
                    options.OptimizeForImmutableData = true;
                })
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting silo...");
            await silo.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (silo != null)
            {
                Console.WriteLine("Stopping silo...");
                await silo.StopAsync(cancellationToken);
                await silo.Stopped;
                Console.WriteLine("Stopped gracefully");
            }
        }
    }
}
