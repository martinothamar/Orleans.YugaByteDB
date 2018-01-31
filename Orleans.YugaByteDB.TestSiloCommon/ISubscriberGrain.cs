using System;
using System.Threading.Tasks;

namespace Orleans.YugaByteDB.TestSiloCommon
{
    public abstract class IntSubscriberGrain<TGrainInterface> : Grain, IIntegerSubscriberGrain
        where TGrainInterface : IIntegerSubscriberGrain
    {
        protected IRawRabbitStreamProvider stream;

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await stream.SubscribeInteger<T, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await stream.Publish(message);
        }
    }

    public abstract class IntSubscriberGrain<TGrainState, TGrainInterface> : Grain<TGrainState>, IIntegerSubscriberGrain
        where TGrainState : new()
        where TGrainInterface : IIntegerSubscriberGrain
    {
        protected IRawRabbitStreamProvider stream;

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await stream.SubscribeInteger<T, TGrainState, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await stream.Publish(message);
        }
    }
    public abstract class GuidSubscriberGrain<TGrainInterface> : Grain, IGuidSubscriberGrain
        where TGrainInterface : IGuidSubscriberGrain
    {
        protected IRawRabbitStreamProvider stream;

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await stream.SubscribeGuid<T, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class
        {
            await stream.Publish(message);
        }
    }

    public abstract class GuidSubscriberGrain<TGrainState, TGrainInterface> : Grain<TGrainState>, IGuidSubscriberGrain
        where TGrainState : new()
        where TGrainInterface : IGuidSubscriberGrain
    {
        protected IRawRabbitStreamProvider stream;

        public abstract Task OnMessage<T>(T message) where T : class;

        public async Task Subscribe<T>(string subscriptionId) where T : class
        {
            var inst = (TGrainInterface)(object)this;
            await stream.SubscribeGuid<T, TGrainState, TGrainInterface>(inst, subscriptionId);
        }

        public async Task Publish<T>(T message) where T : class 
        {
            await stream.Publish(message);
        }
    }

    public interface ISubscriberGrain : IGrain
    {
        Task OnMessage<T>(T message) where T : class;

        Task Subscribe<T>(string subscriptionId) where T : class;

        Task Publish<T>(T message) where T : class;
    }

    public interface IIntegerSubscriberGrain : ISubscriberGrain, IGrainWithIntegerKey
    {
    }
    public interface IGuidSubscriberGrain : ISubscriberGrain, IGrainWithGuidKey
    {
    }
    public interface IStringSubscriberGrain : ISubscriberGrain, IGrainWithStringKey
    {
    }
}
