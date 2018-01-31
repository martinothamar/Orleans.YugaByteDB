using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Providers;
using RawRabbit;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Context;

namespace Orleans.YugaByteDB.TestSiloCommon
{
    public interface IRawRabbitStreamProvider
    {
        // Task Subscribe<TMessage, TGrain>(TGrain grain, string subscriptionId) where TGrain : IPubSubGrain<TMessage> where TMessage : class;

        // Task Subscribe<TGrain, TMessage>(TGrain grain, string subscriptionId)where TGrain : IPubSubGrain<TMessage>, IGrain where TMessage : class;

        // Task Subscribe<TMessage, T>(T grain, string subscriptionId) where T : IPubSubGrain, IGrain where TMessage : class;

        Task Init(IProviderRuntime runtime, IBusClient client = null);

        Task SubscribeInteger<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IIntegerPubSubGrain;

        Task SubscribeInteger<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IIntegerPubSubGrain;

        Task SubscribeGuid<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IGuidPubSubGrain;

        Task SubscribeGuid<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IGuidPubSubGrain;

        Task Publish<TMessage>(TMessage message) where TMessage : class;
    }

    public class RawRabbitSubscription
    {
        public Type MessageType { get; set; }

        public RawRabbitSubscription(Type messageType)
        {
            MessageType = messageType;
        }

        public override bool Equals(object obj)
        {
            if (obj is RawRabbitSubscription sub)
            {
                return MessageType == sub.MessageType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return MessageType.GetHashCode();
        }
    }

    public class RawRabbitStreamProvider : IRawRabbitStreamProvider
    {
        private bool _initialized;
        private IGrainFactory _factory;
        private IBusClient _client;
        private ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IIntegerPubSubGrain>> _integerSubs;
        private ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IGuidPubSubGrain>> _guidSubs;
        private TaskScheduler _orleansScheduler;
        private TaskScheduler _defaultScheduler;

        public Task Init(IProviderRuntime runtime, IBusClient client = null)
        {
            if (_initialized)
                throw new Exception("Provider already initialized");
            _initialized = true;
            _factory = runtime.GrainFactory;
            _orleansScheduler = TaskScheduler.Current;
            _defaultScheduler = TaskScheduler.Default;

            _integerSubs = new ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IIntegerPubSubGrain>>();
            _guidSubs = new ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IGuidPubSubGrain>>();

            _client = client ?? (IBusClient)runtime.ServiceProvider.GetService(typeof(IBusClient));

            return Task.CompletedTask;
        }

        public async Task Handler<T>(T message) where T : class
        {
            var msgType = typeof(T);
            var sub = new RawRabbitSubscription(msgType);
            
            var intSubs = _integerSubs.GetOrAdd(sub, new ConcurrentQueue<IIntegerPubSubGrain>());
            var guidSubs = _guidSubs.GetOrAdd(sub, new ConcurrentQueue<IGuidPubSubGrain>());
            
            var tasks = new Task[intSubs.Count + guidSubs.Count];
            for (int i = 0; i < intSubs.Count; i++)
            {
                var intSub = intSubs.ElementAt(i);
                tasks[i] = OrleansDispatch(async () => await intSub.OnMessage(message));
            }
            for (int i = 0; i < guidSubs.Count; i++)
            {
                var guidSub = guidSubs.ElementAt(i);
                tasks[i] = OrleansDispatch(async () => await guidSub.OnMessage(message));
            }
            await Task.WhenAll(tasks);
        }

        public async Task SubscribeInteger<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IIntegerPubSubGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKeyLong());
            ConcurrentQueue<IIntegerPubSubGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_integerSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IIntegerPubSubGrain>();
                _integerSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await _client.SubscribeAsync((TMessage msg) => Handler(msg));
        }

        public async Task SubscribeInteger<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IIntegerPubSubGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKeyLong());
            ConcurrentQueue<IIntegerPubSubGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_integerSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IIntegerPubSubGrain>();
                _integerSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await _client.SubscribeAsync((TMessage msg) => Handler(msg));
        }

        public async Task SubscribeGuid<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IGuidPubSubGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKey());
            ConcurrentQueue<IGuidPubSubGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_guidSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IGuidPubSubGrain>();
                _guidSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await _client.SubscribeAsync((TMessage msg) => Handler(msg));
        }

        public async Task SubscribeGuid<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IGuidPubSubGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKey());
            ConcurrentQueue<IGuidPubSubGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_guidSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IGuidPubSubGrain>();
                _guidSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await _client.SubscribeAsync((TMessage msg) => Handler(msg));
        }

        public async Task Publish<TMessage>(TMessage message) where TMessage : class
        {
            await _client.PublishAsync(message);
        }

        private Task OrleansDispatch(Func<Task> action)
        {
            return Task.Factory.StartNew(
                action,
                CancellationToken.None,
                TaskCreationOptions.None,
                _orleansScheduler
            ).Unwrap();
        }

        private Task NETDispatch(Func<Task> action)
        {
            return Task.Factory.StartNew(
                action,
                CancellationToken.None,
                TaskCreationOptions.None,
                _defaultScheduler
            ).Unwrap();
        }
    }
}
