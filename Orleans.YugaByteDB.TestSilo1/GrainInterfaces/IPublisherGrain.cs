using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo1.GrainInterfaces
{
    public interface IPublisherGrain : IGuidPubSubGrain
    {
        Task Init();

        Task PublishMessage(object state);
    }
}
