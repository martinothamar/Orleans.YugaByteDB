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
        // Task Subscribe<TMessage, TGrain>(TGrain grain, string subscriptionId) where TGrain : ISubscriberGrain<TMessage> where TMessage : class;

        // Task Subscribe<TGrain, TMessage>(TGrain grain, string subscriptionId)where TGrain : ISubscriberGrain<TMessage>, IGrain where TMessage : class;

        // Task Subscribe<TMessage, T>(T grain, string subscriptionId) where T : ISubscriberGrain, IGrain where TMessage : class;

        Task Init(IProviderRuntime runtime, IBusClient client = null);

        Task SubscribeInteger<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IIntegerSubscriberGrain;

        Task SubscribeInteger<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IIntegerSubscriberGrain;

        Task SubscribeGuid<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IGuidSubscriberGrain;

        Task SubscribeGuid<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IGuidSubscriberGrain;

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
        private ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IIntegerSubscriberGrain>> _integerSubs;
        private ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IGuidSubscriberGrain>> _guidSubs;
        private TaskScheduler _scheduler;
        private TaskScheduler _defaultScheduler;

        public Task Init(IProviderRuntime runtime, IBusClient client = null)
        {
            if (_initialized)
                throw new Exception("Provider already initialized");
            _initialized = true;
            _factory = runtime.GrainFactory;
            _scheduler = TaskScheduler.Current;
            _defaultScheduler = TaskScheduler.Default;

            _integerSubs = new ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IIntegerSubscriberGrain>>();
            _guidSubs = new ConcurrentDictionary<RawRabbitSubscription, ConcurrentQueue<IGuidSubscriberGrain>>();

            _client = client ?? (IBusClient)runtime.ServiceProvider.GetService(typeof(IBusClient));

            return Task.CompletedTask;
        }

        public async Task Handler<T>(T message) where T : class
        {
            Console.WriteLine("GOT MESSAGE: " + message);
            var msgType = typeof(T);
            var sub = new RawRabbitSubscription(msgType);

            Console.WriteLine("subs " + _integerSubs.Count + " " + _guidSubs.Count);
            var intSubs = _integerSubs.GetOrAdd(sub, new ConcurrentQueue<IIntegerSubscriberGrain>());
            var guidSubs = _guidSubs.GetOrAdd(sub, new ConcurrentQueue<IGuidSubscriberGrain>());

            Console.WriteLine(intSubs.Count);
            Console.WriteLine(guidSubs.Count);
            // var tasks = new Task[intSubs.Count + guidSubs.Count];
            for (int i = 0; i < intSubs.Count; i++)
            {
                Console.WriteLine("called intSubs grain");
                var intSub = intSubs.ElementAt(i);
                // tasks[i] = intSub.OnMessage(message);
                Console.WriteLine("called intSubs grain");
            }
            for (int i = 0; i < guidSubs.Count; i++)
            {
                Console.WriteLine("called guidSubs grain");
                var guidSub = guidSubs.ElementAt(i);
                // tasks[intSubs.Count + i] = guidSub.OnMessage(message);
                ;
                await Task.Factory.StartNew(
                    async () => await guidSub.OnMessage(message),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    _scheduler
                 );
                Console.WriteLine("called guidSubs grain");
            }
            // await Task.WhenAll(tasks);
            Console.WriteLine("doneeee");
        }

        public async Task SubscribeInteger<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IIntegerSubscriberGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKeyLong());
            ConcurrentQueue<IIntegerSubscriberGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_integerSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IIntegerSubscriberGrain>();
                _integerSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await Task.Factory.StartNew(
                async () => await _client.SubscribeAsync((TMessage msg) => Handler(msg)), 
                CancellationToken.None,
                TaskCreationOptions.None,
                _defaultScheduler
             );

        }

        public async Task SubscribeInteger<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IIntegerSubscriberGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKeyLong());
            ConcurrentQueue<IIntegerSubscriberGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_integerSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IIntegerSubscriberGrain>();
                _integerSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await Task.Factory.StartNew(
                async () => await _client.SubscribeAsync((TMessage msg) => Handler(msg)),
                CancellationToken.None,
                TaskCreationOptions.None,
                _defaultScheduler
             );
        }

        public async Task SubscribeGuid<TMessage, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainInterface : IGuidSubscriberGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKey());
            ConcurrentQueue<IGuidSubscriberGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_guidSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IGuidSubscriberGrain>();
                _guidSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await Task.Factory.StartNew(
                async () => await _client.SubscribeAsync((TMessage msg) => Handler(msg)),
                CancellationToken.None,
                TaskCreationOptions.None,
                _defaultScheduler
             );
        }

        public async Task SubscribeGuid<TMessage, TGrainState, TGrainInterface>(TGrainInterface grain, string subscriptionId)
            where TMessage : class
            where TGrainState : new()
            where TGrainInterface : IGuidSubscriberGrain
        {
            var actualGrain = _factory.GetGrain<TGrainInterface>(grain.GetPrimaryKey());
            ConcurrentQueue<IGuidSubscriberGrain> queue;
            var subType = new RawRabbitSubscription(typeof(TMessage));
            if (!_guidSubs.TryGetValue(subType, out queue))
            {
                queue = new ConcurrentQueue<IGuidSubscriberGrain>();
                _guidSubs.AddOrUpdate(subType, queue, (subscription, grains) =>
                {
                    queue = grains;
                    return grains;
                });
            }
            queue.Enqueue(actualGrain);
            await Task.Factory.StartNew(
                async () => await _client.SubscribeAsync((TMessage msg) => Handler(msg)),
                CancellationToken.None,
                TaskCreationOptions.None,
                _defaultScheduler
             );
        }

        public async Task Publish<TMessage>(TMessage message) where TMessage : class
        {
            await _client.PublishAsync(message);
        }
    }
}
