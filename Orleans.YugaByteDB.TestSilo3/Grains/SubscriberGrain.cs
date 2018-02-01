using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSilo2.GrainInterfaces;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo2.Grains
{
    public class SubscriberGrain : GuidPubSubGrain<SubscriberGrainState, ISubscriberGrain>, ISubscriberGrain
    {
        public SubscriberGrain(IRawRabbitStreamProvider stream) : base(stream)
        {
        }

        public Task Init()
        {
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            Console.WriteLine($"SubscriberGrain subscribing: {RuntimeIdentity}");
            // await Subscribe<SomeState>("default");
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine($"SubscriberGrain deactivating: {RuntimeIdentity}");
            return base.OnDeactivateAsync();
        }

        private int counter = 0;
        private double started = DateTime.UtcNow.Ticks;

        public override Task OnMessage<T>(T message)
        {
            var velocity = "";
            if (counter > 0)
            {
                var now = DateTime.UtcNow.Ticks;
                var diff = now - started;
                var seconds = diff / TimeSpan.TicksPerSecond;
                velocity = (counter / seconds).ToString("#.000");
            }
            Console.Write($"\rRecieving! --------- {counter++} {velocity}/s --------- {RuntimeIdentity}     ");
            return Task.CompletedTask;
        }
    }
}
