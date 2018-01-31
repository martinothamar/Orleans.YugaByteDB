using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSilo1.GrainInterfaces;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo1.Grains
{
    public class PublisherGrain : GuidPubSubGrain<PublisherGrainState, IPublisherGrain>, IPublisherGrain
    {
        public PublisherGrain(IRawRabbitStreamProvider stream) : base(stream)
        {
        }

        public Task Init()
        {
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            Console.WriteLine("PublisherGrain publishing");
            RegisterTimer(Publish, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public async Task Publish(object state)
        {
            await Publish(new SomeState());
        }

        public override async Task OnMessage<T>(T message)
        {
            throw new NotImplementedException();
        }
    }
}
