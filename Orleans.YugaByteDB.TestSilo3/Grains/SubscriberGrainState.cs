using System;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo2.Grains
{
    public class SubscriberGrainState
    {
        public SomeState Date { get; set; }
    }
}
