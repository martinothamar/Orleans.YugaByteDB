﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.YugaByteDB.StorageProvider;
using Orleans.YugaByteDB.TestSilo2.Grains;
using Orleans.YugaByteDB.TestSiloCommon;
using OrleansDashboard;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Context;
using RawRabbit.Instantiation;

namespace Orleans.YugaByteDB.TestSilo2
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
            var config = ClusterConfiguration.LocalhostPrimarySilo(22223, 40001);
            var opts = new Dictionary<string, string>()
            {
                { "Serializer", RedisStorageProvider.Serializers.Protobuf },
                { "DataConnectionString", "127.0.0.1" }
            };
            config.Globals.RegisterStorageProvider<RedisStorageProvider>("Default", opts);
            config.Globals.RegisterBootstrapProvider<Startup>(nameof(Startup));
            config.Globals.ClusterId = "ConsumerCluster";
            config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.ZooKeeper;
            config.Globals.LivenessEnabled = true;
            config.Globals.ServiceId = Guid.Parse("68e89d8e-60d5-43ba-992e-4759d4696968");
            config.Globals.DataConnectionString = "localhost:2181";
            config.Globals.RegisterDashboard(8080);

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
                .ConfigureApplicationParts(parts => 
                {
                    parts.AddApplicationPart(typeof(SubscriberGrain).Assembly);
                    parts.AddApplicationPart(typeof(Dashboard).Assembly);
            });
            builder.ConfigureServices(x => x.AddDashboard(y => y.Port = 8080));

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}