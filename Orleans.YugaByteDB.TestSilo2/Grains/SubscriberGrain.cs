using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSilo2.GrainInterfaces;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo2.Grains
{
    public class SubscriberGrain : GuidSubscriberGrain<SubscriberGrainState, Orleans.YugaByteDB.TestSilo2.GrainInterfaces.ISubscriberGrain>, Orleans.YugaByteDB.TestSilo2.GrainInterfaces.ISubscriberGrain
    {
        public SubscriberGrain(IRawRabbitStreamProvider stream)
        {
            this.stream = stream;
        }

        public Task Init()
        {
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            Console.WriteLine("SubscriberGrain subscribing");
            await Subscribe<SomeState>("default");
        }

        public override Task OnMessage<T>(T message)
        {
            Console.WriteLine("SubscriberGrain Message: " + message);
            return Task.CompletedTask;
        }
    }
}
