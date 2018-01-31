using System;
using System.Threading.Tasks;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo2.GrainInterfaces
{
    public interface ISubscriberGrain : IGuidSubscriberGrain
    {
        Task Init();
    }
}
