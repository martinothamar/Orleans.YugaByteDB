using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.YugaByteDB.StorageProvider;
using Orleans.YugaByteDB.TestSilo1.Grains;
using Orleans.YugaByteDB.TestSiloCommon;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Context;
using RawRabbit.Instantiation;

namespace Orleans.YugaByteDB.TestSilo1
{
    class Program
    {
        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                Console.WriteLine("Stopping...");
                await host.StopAsync();
                Console.WriteLine("Stopped");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var config = ClusterConfiguration.LocalhostPrimarySilo();
            var opts = new Dictionary<string, string>()
            {
                { "Serializer", RedisStorageProvider.Serializers.Protobuf },
                { "DataConnectionString", "127.0.0.1" }
            };
            config.Globals.RegisterStorageProvider<RedisStorageProvider>("Default", opts);
            config.Globals.RegisterBootstrapProvider<Startup>(nameof(Startup));

            var client = RawRabbitFactory.CreateSingleton(new RawRabbitOptions
            {
                ClientConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("rawrabbit.json")
                    .Build()
                    .Get<RawRabbitConfiguration>(),
                Plugins = p => p
                    .UseProtobuf()
            });
            
            var builder = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IRawRabbitStreamProvider, RawRabbitStreamProvider>();
                    services.AddSingleton<IBusClient>(client);
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(PublisherGrain).Assembly))
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
