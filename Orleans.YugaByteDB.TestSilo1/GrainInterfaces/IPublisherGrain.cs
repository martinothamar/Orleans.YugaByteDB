using System;
using System.Threading.Tasks;

namespace Orleans.YugaByteDB.TestSilo1.GrainInterfaces
{
    public interface IPublisherGrain : IGrainWithGuidKey
    {
        Task Init();

        Task Publish(object state);
    }
}
