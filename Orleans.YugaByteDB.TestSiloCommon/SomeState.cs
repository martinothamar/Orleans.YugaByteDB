using System;
using ProtoBuf;

namespace Orleans.YugaByteDB.TestSiloCommon
{
    [ProtoContract]
    public class SomeState
    {
        [ProtoMember(1)]
        public string Property1 { get; set; } = "Property1";
        [ProtoMember(2)]
        public string Property2 { get; set; } = "Property2";
        [ProtoMember(3)]
        public string Property3 { get; set; } = "Property3";
        [ProtoMember(4)]
        public bool Property4 { get; set; }
        [ProtoMember(5)]
        public bool Property5 { get; set; } = true;
        [ProtoMember(6)]
        public int Property6 { get; set; } = 1231230121;
        [ProtoMember(7)]
        public decimal Property7 { get; set; } = 10.0001m;
        [ProtoMember(8)]
        public long Property8 { get; set; } = 123302L;
        [ProtoMember(9)]
        public float Property9 { get; set; } = 1.1f;
        [ProtoMember(10)]
        public double Property10 { get; set; } = 20d;

        public override bool Equals(object obj)
        {
            if (obj is SomeState otherState)
            {
                return (
                    Property1 == otherState.Property1 &&
                    Property2 == otherState.Property2 &&
                    Property3 == otherState.Property3 &&
                    Property4 == otherState.Property4 &&
                    Property5 == otherState.Property5 &&
                    Property6 == otherState.Property6 &&
                    Property7 == otherState.Property7 &&
                    Property8 == otherState.Property8 &&
                    Property9 == otherState.Property9 &&
                    Property10 == otherState.Property10
                );
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Property1.GetHashCode();
            hash = (hash * 7) + Property2.GetHashCode();
            hash = (hash * 7) + Property3.GetHashCode();
            return hash;
        }
    }
}
