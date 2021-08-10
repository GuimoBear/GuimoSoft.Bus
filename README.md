# GuimoSoft.Bus

Este projeto fornece uma abstração da comunicação entre brokers e uma implementação do Mediator, tem por finalidade facilitar a integração de um projeto a comunicação com tais brokers.

# Documentação

Caso queira entender como utilizar a solução em seu projeto, acesse [nossa documentação](./docs/README.md).

## Benchmark

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1110 (20H2/October2020Update)
Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.200
  [Host]   : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT
  ShortRun : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT


```
|                                     Implementation |     Mean |    StdDev |     Error |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------------------------------- |---------:|----------:|----------:|-------:|------:|------:|----------:|
|  Consuming directly from IConsumer&lt;string, byte[]&gt; | 1.205 μs | 0.1565 μs | 0.2629 μs | 0.1836 |     - |     - |     768 B |
|                      Consuming and send to MediatR | 1.473 μs | 0.0146 μs | 0.0221 μs | 0.2852 |     - |     - |   1,200 B |
| Consuming and send via MediatorPublisherMiddleware | 1.731 μs | 0.0162 μs | 0.0272 μs | 0.3711 |     - |     - |   1,560 B |
|                   Consuming using all Bus features | 3.443 μs | 0.1870 μs | 0.2827 μs | 0.5273 |     - |     - |   2,229 B |

## Requisitos

### Instalação do .NET Core SDK >= 5.0

Esta API depende do [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet/5.0), com versão igual ou superior ao 5.0.

## Instalação dos pacotes

### GuimoSoft.Bus.Abstractions
```
Install-Package GuimoSoft.Bus.Abstractions
```

### GuimoSoft.Bus.Kafka
```
Install-Package GuimoSoft.Bus.Kafka
```
