using System;
using System.Threading.Tasks;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.YugaByteDB.TestSilo2.GrainInterfaces;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo2
{

    public class Startup : IBootstrapProvider
    {
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            try
            {
                Console.WriteLine("Starting");
                this.Name = name;

                var pubsub = (IRawRabbitStreamProvider)providerRuntime.ServiceProvider.GetService(typeof(IRawRabbitStreamProvider));
                await pubsub.Init(providerRuntime);

                /*
                for (int i = 0; i < 5; i++) {
                    var grain = providerRuntime.GrainFactory.GetGrain<ISubscriberGrain>(Guid.NewGuid());
                    await grain.Init();
                }
                */
                Console.WriteLine("Started");


                var manager = providerRuntime.GrainFactory.GetGrain<IManagementGrain>(0);

                var hosts = await manager.GetDetailedHosts(true);
                foreach (var host in hosts) {
                    Console.WriteLine("Host: " + host.SiloName + ", address: " + host.SiloAddress.ToLongString());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Startup.Init - error starting silo");
            }
        }

        public Task Close() => Task.FromResult(0);

        public string Name { get; set; }
    }
}
