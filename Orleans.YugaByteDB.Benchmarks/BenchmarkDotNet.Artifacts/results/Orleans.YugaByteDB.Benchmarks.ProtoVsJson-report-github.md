``` ini

BenchmarkDotNet=v0.10.12, OS=macOS 10.13.2 (17C88) [Darwin 17.3.0]
Intel Core i7-4770HQ CPU 2.20GHz (Haswell), 1 CPU, 8 logical cores and 4 physical cores
.NET Core SDK=2.0.3
  [Host]     : .NET Core 2.0.3 (Framework 4.6.0.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.0.0), 64bit RyuJIT


```
|                 Method |       Mean |     Error |     StdDev |  Gen 0 | Allocated |
|----------------------- |-----------:|----------:|-----------:|-------:|----------:|
|     YugaByte_JsonWrite | 2,562.5 us | 50.881 us | 120.924 us |      - |    3577 B |
| YugaByte_ProtobufWrite | 2,427.7 us | 48.483 us |  64.724 us |      - |    2266 B |
|      YugaByte_JsonRead |   905.6 us | 18.140 us |  36.227 us |      - |     968 B |
|  YugaByte_ProtobufRead |   906.9 us | 17.928 us |  17.607 us |      - |     984 B |
|        Redis_JsonWrite |   137.0 us |  2.684 us |   3.490 us | 0.2441 |    2280 B |
|    Redis_ProtobufWrite |   136.6 us |  2.731 us |   4.637 us | 0.2441 |    2280 B |
|         Redis_JsonRead |   116.1 us |  2.557 us |   3.414 us | 0.1221 |     984 B |
|     Redis_ProtobufRead |   114.6 us |  2.282 us |   3.046 us | 0.1221 |     984 B |
