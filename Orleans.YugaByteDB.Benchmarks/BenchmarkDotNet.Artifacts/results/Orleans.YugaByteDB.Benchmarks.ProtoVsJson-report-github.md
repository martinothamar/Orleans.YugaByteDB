``` ini

BenchmarkDotNet=v0.10.12, OS=macOS 10.13.2 (17C88) [Darwin 17.3.0]
Intel Core i7-4770HQ CPU 2.20GHz (Haswell), 1 CPU, 8 logical cores and 4 physical cores
.NET Core SDK=2.0.3
  [Host]     : .NET Core 2.0.3 (Framework 4.6.0.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.0.0), 64bit RyuJIT


```
|                 Method |       Mean |     Error |     StdDev | Allocated |
|----------------------- |-----------:|----------:|-----------:|----------:|
|     YugaByte_JsonWrite | 2,764.0 us | 54.053 us |  64.347 us |    4089 B |
| YugaByte_ProtobufWrite | 2,546.1 us | 49.930 us | 100.860 us |    2282 B |
|      YugaByte_JsonRead |   883.8 us | 17.580 us |  18.810 us |     968 B |
|  YugaByte_ProtobufRead |   926.1 us | 18.389 us |  20.439 us |     984 B |
|        Redis_JsonWrite |   459.0 us |  6.760 us |   5.992 us |    2264 B |
|    Redis_ProtobufWrite |   423.4 us |  8.233 us |   9.481 us |    2280 B |
|         Redis_JsonRead |   430.3 us |  8.479 us |  11.607 us |     968 B |
|     Redis_ProtobufRead |   427.8 us |  9.320 us |   9.153 us |     984 B |
