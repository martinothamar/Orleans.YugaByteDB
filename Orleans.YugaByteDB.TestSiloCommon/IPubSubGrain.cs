using System;
using System.Threading.Tasks;

namespace Orleans.YugaByteDB.TestSiloCommon
{
    public abstract class IntPubSubGrain<TGrainInterface> : Grain, IIntegerPubSubGrain
        where TGrainInterface : IIntegerPubSubGrain
    {
        private readonly IRawRabbitStreamProvider _stream;

        protected IntPubSubGrain(IRawRabbitStreamProvider stream)
        {
            _stream = stream;
        }

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await _stream.SubscribeInteger<T, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await _stream.Publish(message);
        }
    }

    public abstract class IntPubSubGrain<TGrainState, TGrainInterface> : Grain<TGrainState>, IIntegerPubSubGrain
        where TGrainState : new()
        where TGrainInterface : IIntegerPubSubGrain
    {
        private readonly IRawRabbitStreamProvider _stream;

        protected IntPubSubGrain(IRawRabbitStreamProvider stream)
        {
            _stream = stream;
        }

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await _stream.SubscribeInteger<T, TGrainState, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await _stream.Publish(message);
        }
    }
    public abstract class GuidPubSubGrain<TGrainInterface> : Grain, IGuidPubSubGrain
        where TGrainInterface : IGuidPubSubGrain
    {
        private readonly IRawRabbitStreamProvider _stream;

        protected GuidPubSubGrain(IRawRabbitStreamProvider stream)
        {
            _stream = stream;
        }

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await _stream.SubscribeGuid<T, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await _stream.Publish(message);
        }
    }

    public abstract class GuidPubSubGrain<TGrainState, TGrainInterface> : Grain<TGrainState>, IGuidPubSubGrain
        where TGrainState : new()
        where TGrainInterface : IGuidPubSubGrain
    {
        private readonly IRawRabbitStreamProvider _stream;

        protected GuidPubSubGrain(IRawRabbitStreamProvider stream)
        {
            _stream = stream;
        }

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await _stream.SubscribeGuid<T, TGrainState, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class 
        {
            await _stream.Publish(message);
        }
    }

    public interface IPubSubGrain : IGrain
    {
        Task OnMessage<T>(T message) where T : class;

        Task Subscribe<T>(string subscriptionId) where T : class;

        Task Publish<T>(T message) where T : class;
    }

    public interface IIntegerPubSubGrain : IPubSubGrain, IGrainWithIntegerKey
    {
    }
    public interface IGuidPubSubGrain : IPubSubGrain, IGrainWithGuidKey
    {
    }
    public interface IStringPubSubGrain : IPubSubGrain, IGrainWithStringKey
    {
    }
}
