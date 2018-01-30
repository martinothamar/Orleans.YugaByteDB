using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using StackExchange.Redis;

namespace Orleans.YugaByteDB.StorageProvider
{
    public class RedisStorageProvider : IStorageProvider
    {
        public Logger Log { get; set; }

        public string Name { get; set; }

        public static class Serializers 
        {
            public const string Json = "Json";
            public const string Protobuf = "Protobuf";

            public static bool IsValid(string val) 
            {
                var props = typeof(Serializers).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();

                foreach (var prop in props) 
                {
                    var propValue = (string)prop.GetRawConstantValue();
                    if (propValue == val)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private string _serializer;
        private int _dbNumber = -1;
        private string _connectionString;
        private string _host;
        private int _port;

        private IConnectionMultiplexer _connection;
        private IDatabaseAsync _db;

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            Log = providerRuntime.GetLogger(nameof(RedisStorageProvider));

            if (config.Properties == null)
                throw new ArgumentException("No configuration given");

            if (!config.Properties.TryGetValue("DataConnectionString", out _connectionString))
                throw new ArgumentException("DataConnectionString must be configured for RedisStorageProvider");
            else if (string.IsNullOrWhiteSpace(_connectionString))
                throw new ArgumentException("DataConnectionString was null or whitespace for RedisStorageProvider");

            var serverAndPort = _connectionString.Split(':');
            if (serverAndPort.Length == 1) {
                _host = serverAndPort[0];
                _port = 6379;
            } else if (serverAndPort.Length == 2) {
                _host = serverAndPort[0];
                if (!int.TryParse(serverAndPort[1], out _port)) {
                    throw new ArgumentException($"Couldn't parse port from DataConnectionString: {_connectionString}");
                }
            }

            if (!config.Properties.TryGetValue("Serializer", out _serializer))
                _serializer = Serializers.Json;
            else if (!Serializers.IsValid(_serializer))
                throw new ArgumentException($"Serializer from configuration not valid: {_serializer}");
            
            if (config.Properties.TryGetValue("DatabaseNumber", out var dbNumber))
            {
                if (!int.TryParse(dbNumber, out _dbNumber)) 
                {
                    throw new ArgumentException($"DatabaseNumber has illegal value in configuration: {dbNumber}");
                }
            }

            try {
                var connCfg = new ConfigurationOptions
                {
                    EndPoints =
                    {
                       { _host, _port },
                    },
                    CommandMap = CommandMap.Create(new HashSet<string>
                    { // EXCLUDE commands not supported by YugaByte
                      "SUBSCRIBE", "CLUSTER", "PING", "TIME"
                    }, available: false)
                };
                var diConn = providerRuntime.ServiceProvider.GetService(typeof(IConnectionMultiplexer)) as IConnectionMultiplexer;
                _connection = diConn ?? await ConnectionMultiplexer.ConnectAsync(connCfg);
                _db = _connection.GetDatabase(_dbNumber);
            } catch (RedisConnectionException ex) {
                throw new Exception($"Could not connect to Redis at: {_connectionString}", ex);
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var key = GetKey(grainType, grainReference, grainState);
            await _db.KeyDeleteAsync(key);
        }

        public Task Close()
        {
            _connection.Dispose();
            _connection = null;
            _db = null;
            return Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var key = GetKey(grainType, grainReference, grainState);

            var content = await _db.StringGetAsync(key);
            if (!content.HasValue)
                return;

            if (_serializer == Serializers.Json)
                grainState.State = JsonConvert.DeserializeObject(content, grainState.State.GetType());
            else if (_serializer == Serializers.Protobuf)
            {
                var bytes = Convert.FromBase64String(content);
                using(var stream = new MemoryStream(bytes))
                {
                    grainState.State = ProtoBuf.Serializer.Deserialize(grainState.State.GetType(), stream);
                }
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var key = GetKey(grainType, grainReference, grainState);

            string content = null;
            var state = grainState.State;

            if (_serializer == Serializers.Json)
                content = JsonConvert.SerializeObject(state);
            else if (_serializer == Serializers.Protobuf)
            {
                using (var stream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(stream, state);
                    content = Convert.ToBase64String(stream.ToArray());
                }
            }
            await _db.StringSetAsync(key, content);
        }

        private string GetKey(string grainType, GrainReference grainReference, IGrainState grainState) 
        {
            var collectionName = grainState.GetType().FullName;
            var grainKey = grainReference?.ToKeyString() ?? grainType;
            var key = grainKey + "." + collectionName;
            return key;
        }
    }
}
