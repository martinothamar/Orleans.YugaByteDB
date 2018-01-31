﻿using System;
using System.Threading.Tasks;
using Orleans.Providers;
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

                var grain = providerRuntime.GrainFactory.GetGrain<Orleans.YugaByteDB.TestSilo2.GrainInterfaces.ISubscriberGrain>(Guid.Empty);
                await grain.Init();
                Console.WriteLine("Started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Startup.Init - error starting silo");
            }
        }

        public Task Close() => Task.FromResult(0);

        public string Name { get; set; }
    }
}