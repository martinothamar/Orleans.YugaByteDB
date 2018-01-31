using System;
using Orleans.YugaByteDB.TestSiloCommon;

namespace Orleans.YugaByteDB.TestSilo1.Grains
{
    public class PublisherGrainState
    {
        public SomeState Data { get; set; }
    }
}
