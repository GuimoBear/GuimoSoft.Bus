``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1110 (20H2/October2020Update)
Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.200
  [Host]   : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT
  ShortRun : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT


```
|                                     Implementation |     Mean |    StdDev |     Error |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------------------------------- |---------:|----------:|----------:|-------:|------:|------:|----------:|
|  Consuming directly from IConsumer&lt;string, byte[]&gt; | 1.175 μs | 0.0488 μs | 0.0820 μs | 0.1836 |     - |     - |     768 B |
|                      Consuming and send to MediatR | 1.535 μs | 0.0185 μs | 0.0280 μs | 0.2852 |     - |     - |   1,200 B |
| Consuming and send via MediatorPublisherMiddleware | 1.828 μs | 0.0157 μs | 0.0238 μs | 0.3711 |     - |     - |   1,560 B |
|                   Consuming using all Bus features | 3.437 μs | 0.1260 μs | 0.2117 μs | 0.5313 |     - |     - |   2,243 B |
