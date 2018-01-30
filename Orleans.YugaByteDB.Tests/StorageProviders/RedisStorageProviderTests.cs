using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.YugaByteDB.StorageProvider;
using ProtoBuf;
using Xunit;

namespace Orleans.YugaByteDB.Tests
{
    public class RedisStorageProviderTests
    {
        [Theory]
        [InlineData("asddd", false)]
        [InlineData("dsaaa", false)]
        [InlineData(RedisStorageProvider.Serializers.Json, true)]
        [InlineData(RedisStorageProvider.Serializers.Protobuf, true)]
        public void Test_Serializers_Validator(string data, bool expected)
        {
            RedisStorageProvider.Serializers.IsValid(data).Should().Be(expected);
        }

        [Fact]
        public async Task Test_Initialization()
        {
            var logger = new Mock<Logger>();
            var runtime = new Mock<IProviderRuntime>();
            runtime.SetupSequence(r => r.GetLogger(It.IsAny<string>()))
                   .Returns(null)
                   .Returns(logger.Object);
            var serviceProvider = new Mock<IServiceProvider>();
            runtime.SetupGet(r => r.ServiceProvider)
                   .Returns(serviceProvider.Object);

            var config = new Mock<IProviderConfiguration>();
            config.SetupGet(c => c.Properties).Returns(default(ReadOnlyDictionary<string, string>));

            var provider = new RedisStorageProvider();

            Func<Task> f = null;
            Exception ex = null;

            f = async () => await provider.Init("store", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "No configuration given");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store");

            f = async () => await provider.Init("store1", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "No configuration given");
            provider.Log.ShouldBeEquivalentTo(logger.Object);
            provider.Name.Should().Be("store1");


            var cfg = new Dictionary<string, string>();
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            f = async () => await provider.Init("store2", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "DataConnectionString must be configured for RedisStorageProvider");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store2");


            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "";
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            f = async () => await provider.Init("store2", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "DataConnectionString was null or whitespace for RedisStorageProvider");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store2");


            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "localhost";
            cfg["Serializer"] = "asd";
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            f = async () => await provider.Init("store2", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "Serializer from configuration not valid: asd");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store2");


            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "localhost";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Protobuf;
            cfg["DatabaseNumber"] = "asd";
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            f = async () => await provider.Init("store2", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, "DatabaseNumber has illegal value in configuration: asd");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store2");


            cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "localhost:a32";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Protobuf;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            f = async () => await provider.Init("store2", runtime.Object, config.Object);
            ex = await Record.ExceptionAsync(f);
            Assert.IsType(typeof(ArgumentException), ex);
            Assert.Equal(ex.Message, $"Couldn't parse port from DataConnectionString: localhost:a32");
            provider.Log.Should().BeNull();
            provider.Name.Should().Be("store2");
        }

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

        [Fact/*(Skip = "Run when testing real connection to Redis")*/]
        public async Task TestConnectionLocalRedis() 
        {
            var logger = new Mock<Logger>();
            var runtime = new Mock<IProviderRuntime>();
            runtime.Setup(r => r.GetLogger(It.IsAny<string>()))
                   .Returns(logger.Object);
            var serviceProvider = new Mock<IServiceProvider>();
            runtime.SetupGet(r => r.ServiceProvider)
                   .Returns(serviceProvider.Object);

            var config = new Mock<IProviderConfiguration>();
            config.SetupGet(c => c.Properties).Returns(default(ReadOnlyDictionary<string, string>));

            var provider = new RedisStorageProvider();

            var cfg = new Dictionary<string, string>();
            cfg["DataConnectionString"] = "127.0.0.1";
            cfg["Serializer"] = RedisStorageProvider.Serializers.Protobuf;
            config.SetupGet(c => c.Properties).Returns(new ReadOnlyDictionary<string, string>(cfg));

            await provider.Init("store2", runtime.Object, config.Object);

            var defaultStateObj = new SomeState();

            var stateMock = new Mock<IGrainState>();
            var stateObj = new SomeState();

            Assert.True(stateObj.Equals(defaultStateObj));
            Assert.Equal(stateObj.GetHashCode(), defaultStateObj.GetHashCode());

            var state = stateMock.Object;
            stateMock.SetupGet(s => s.State)
                     .Returns(stateObj);

            await provider.ReadStateAsync("proto123123123", null, state);
            stateMock.VerifySet(s => s.State = It.IsAny<object>(), Times.Never());

            await provider.WriteStateAsync("proto", null, state);
            stateMock.VerifyGet(s => s.State, Times.Once());

            await provider.ReadStateAsync("proto", null, state);
            stateMock.VerifySet(s => s.State = It.Is<SomeState>(o => o.Equals(stateObj)), Times.Once());
        }
    }
}
