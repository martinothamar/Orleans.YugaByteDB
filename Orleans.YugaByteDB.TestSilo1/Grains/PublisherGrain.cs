using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSilo1.GrainInterfaces;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo1.Grains
{
    public class PublisherGrain : Grain<PublisherGrainState>, IPublisherGrain
    {
        private readonly IRawRabbitStreamProvider _stream;

        public PublisherGrain(IRawRabbitStreamProvider stream)
        {
            _stream = stream;
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
            await _stream.Publish(new SomeState());
        }
    }
}
