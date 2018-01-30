using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FluentAssertions;
using Moq;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.YugaByteDB.StorageProvider;
using ProtoBuf;
using StackExchange.Redis;

namespace Orleans.YugaByteDB.Benchmarks
{
    [MemoryDiagnoser]
    public class ProtoVsJson
    {
        private RedisStorageProvider _yugaByte_jsonProvider;
        private RedisStorageProvider _yugaByte_protoProvider;

        private RedisStorageProvider _redis_jsonProvider;
        private RedisStorageProvider _redis_protoProvider;

        private IGrainState _state;

        [ProtoContract]
        public class SomeState
        {
            [ProtoMember(1)]
            public string Property1 { get; set; } = "Property1";
            [ProtoMember(2)]
            public string Property2 { get; set; } = "Property2";
            [ProtoMember(3)]
            public string Property3 { get; set; } = "Property3";
            [ProtoMember(4)]
            public bool Property4 { get; set; }
            [ProtoMember(5)]
            public bool Property5 { get; set; } = true;
            [ProtoMember(6)]
            public int Property6 { get; set; } = 1231230121;
            [ProtoMember(7)]
            public decimal Property7 { get; set; } = 10.0001m;
            [ProtoMember(8)]
            public long Property8 { get; set; } = 123302L;
            [ProtoMember(9)]
            public float Property9 { get; set; } = 1.1f;
            [ProtoMember(10)]
            public double Property10 { get; set; } = 20d;

            public override bool Equals(object obj)
            {
                if (obj is SomeState otherState)
                {
                    return (
                        Property1 == otherState.Property1 &&
                        Property2 == otherState.Property2 &&
                        Property3 == otherState.Property3 &&
                        Property4 == otherState.Property4 &&
                        Property5 == otherState.Property5 &&
                        Property6 == otherState.Property6 &&
                        Property7 == otherState.Property7 &&
                        Property8 == otherState.Property8 &&
                        Property9 == otherState.Property9 &&
                        Property10 == otherState.Property10
                    );
                }
                return false;
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + Property1.GetHashCode();
                hash = (hash * 7) + Property2.GetHashCode();
                hash = (hash * 7) + Property3.GetHashCode();
                return hash;
            }
        }

        public ProtoVsJson()
        {
            var logger = new Mock<Logger>();
            var runtime = new Mock<IProviderRuntime>();
            runtime.Setup(r => r.GetLogger(It.IsAny<string>()))
                   .Returns(logger.Object);
            var serviceProvider = new Mock<IServiceProvider>();
            runtime.SetupGet(r => r.ServiceProvider)
                   .Returns(serviceProvider.Object);


            var yugaByteRedisConnCfg = new ConfigurationOptions
            {
                EndPoints =
                {
                   { "127.0.0.1", 6379 },
                },
                CommandMap = CommandMap.Create(new HashSet<string>
                { // EXCLUDE commands not supported by YugaByte
                  "SUBSCRIBE", "CLUSTER", "PING", "TIME"
                }, available: false)
            };
            var rawRedisConnCfg = new ConfigurationOptions
            {
                EndPoints =
                {
                   { "127.0.0.1", 6380 },
                },
                CommandMap = CommandMap.Create(new HashSet<string>
                { // EXCLUDE commands not supported by YugaByte
                  "SUBSCRIBE", "CLUSTER", "PING", "TIME"
                }, available: false)
            };

            var yugaByteConnection = ConnectionMultiplexer.Connect(yugaByteRedisConnCfg);
            var redisConnection = ConnectionMultiplexer.Connect(rawRedisConnCfg);
            var database = yugaByteConnection.GetDatabase();
            database.Execute("FLUSHALL").IsNull.Should().BeFalse();
            database = redisConnection.GetDatabase();
            database.Execute("FLUSHALL").IsNull.Should().BeFalse();

            var config = new Mock<IProviderConfiguration>();
            var cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "127.0.0.1:6379";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Json;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));
            serviceProvider
                .Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IConnectionMultiplexer))))
                .Returns(yugaByteConnection)
                .Verifiable();

            _yugaByte_jsonProvider = new RedisStorageProvider();
            _yugaByte_jsonProvider.Init("json", runtime.Object, config.Object).Wait();
            serviceProvider.Verify();

            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "127.0.0.1:6379";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Protobuf;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            _yugaByte_protoProvider = new RedisStorageProvider();
            _yugaByte_protoProvider.Init("proto", runtime.Object, config.Object).Wait();
            serviceProvider.Verify();

            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "127.0.0.1:6380";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Json;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));
            serviceProvider
                .Setup(c => c.GetService(It.Is<Type>(t => t == typeof(IConnectionMultiplexer))))
                .Returns(redisConnection)
                .Verifiable();

            _redis_jsonProvider = new RedisStorageProvider();
            _redis_jsonProvider.Init("json", runtime.Object, config.Object).Wait();
            serviceProvider.Verify();

            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "127.0.0.1:6380";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Protobuf;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            _redis_protoProvider = new RedisStorageProvider();
            _redis_protoProvider.Init("proto", runtime.Object, config.Object).Wait();
            serviceProvider.Verify();

            var stateMock = new Mock<IGrainState>();
            var state = new SomeState();
            stateMock.SetupGet(s => s.State)
                     .Returns(state);
            _state = stateMock.Object;
        }

        private int _yugaByte_writeCounter = 1;
        private int _yugaByte_readCounter = 1;

        private int _redis_writeCounter = 1;
        private int _redis_readCounter = 1;

        [Benchmark]
        public async Task YugaByte_JsonWrite() 
        {
            await _yugaByte_jsonProvider.WriteStateAsync($"jsonGrain-{_yugaByte_writeCounter++}", null, _state);
        }

        [Benchmark]
        public async Task YugaByte_ProtobufWrite() 
        {
            await _yugaByte_protoProvider.WriteStateAsync($"protoGrain-{_yugaByte_writeCounter++}", null, _state);
        }

        [Benchmark]
        public async Task YugaByte_JsonRead()
        {
            await _yugaByte_jsonProvider.ReadStateAsync($"jsonGrain-{_yugaByte_readCounter++}", null, _state);
        }

        [Benchmark]
        public async Task YugaByte_ProtobufRead()
        {
            await _yugaByte_protoProvider.ReadStateAsync($"protoGrain-{_yugaByte_readCounter++}", null, _state);
        }

        [Benchmark]
        public async Task Redis_JsonWrite()
        {
            await _redis_protoProvider.WriteStateAsync($"jsonGrain-{_redis_writeCounter++}", null, _state);
        }

        [Benchmark]
        public async Task Redis_ProtobufWrite()
        {
            await _redis_protoProvider.WriteStateAsync($"protoGrain-{_redis_writeCounter++}", null, _state);
        }

        [Benchmark]
        public async Task Redis_JsonRead()
        {
            await _redis_protoProvider.ReadStateAsync($"jsonGrain-{_redis_readCounter++}", null, _state);
        }

        [Benchmark]
        public async Task Redis_ProtobufRead()
        {
            await _redis_protoProvider.ReadStateAsync($"protoGrain-{_redis_readCounter++}", null, _state);
        }
    }
}
