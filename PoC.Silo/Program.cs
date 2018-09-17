using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
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
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences()
                )
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
