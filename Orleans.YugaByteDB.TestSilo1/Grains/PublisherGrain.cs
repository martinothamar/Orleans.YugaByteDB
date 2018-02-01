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
            Console.WriteLine("PublisherGrain publishing\n");
            await PublishMessage(null);
        }

        public async Task PublishMessage(object state)
        {
            var started = DateTime.UtcNow.Ticks;
            for (double i = 0; i < 1000000; i++) {
                var velocity = "0";
                if (i > 0) {
                    var now = DateTime.UtcNow.Ticks;
                    var diff = now - started;
                    var seconds = diff / TimeSpan.TicksPerSecond;
                    velocity = (i / seconds).ToString("#.000");
                }
                Console.Write($"\rPublishing! --------- {i} {velocity}/s         ");
                await Publish(new SomeState());
            }
        }

        public override async Task OnMessage<T>(T message)
        {
            throw new NotImplementedException();
        }
    }
}
