using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Hosting;
using PoC.Grains.Implementation;

namespace PoC.Silo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Start silo
            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .ConfigureApplicationParts(parts => 
                    parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences()
                )
                .Build();

            Console.WriteLine("Starting silo...");

            using (silo) 
            {
                await silo.StartAsync();
                await silo.Stopped;
            }
        }
    }
}
