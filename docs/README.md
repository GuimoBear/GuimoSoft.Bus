# GuimoSoft.Core.Bus

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

O Projeto de Bus do GuimoSoft tem a finalidade de facilitar o trabalho com mensagens em brokers.

Foi desenhado para que a integração da solução com estes brokers sejam feitas de forma descritiva e de fácil leitura, afim de tornar estas integrações mais simples e amigáveis.

> Caso queira comparar o uso da lib com o consumo direto do broker, veja nosso projeto de benchmark e o [resultado de sua execução](./benchmark.md)

O projeto é dividido em dois pacotes:

## 1. GuimoSoft.Bus.Abstractions

Neste pacote residem as abstrações necessárias para integração do domínio da aplicação com o Bus, ele depende diretamente do [MediatR](https://github.com/jbogard/MediatR).

### Existem quatro interfaces principais que estão nesta LIB

```csharp
public interface IMessage : INotification
```

Esta interface deve ser implementada em todas as mensagens que serão transitadas pelo Bus, seja para seu consumo ou para a sua produção, é apenas uma assinatura, não existe nada a ser implementado.

```csharp
public interface IMessageMiddleware<TType> 
    where TType : IMessage
```

Esta interface contém o contrato do que é um middleware na pipeline de consumo de uma mensagem no Bus, mais a frente mostraremos como registrá-la e como ordená-la dentro desta pipeline.

```csharp
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
```

Esta interface equivale a última etapa da pipeline, será chamada quando o Bus identificar um evendo e nenhum middleware pare o fluxo de execução da pipeline.

```csharp
public interface IConsumeContextAccessor<TMessage> 
    where TMessage : IMessage
```

Esta interface possibilita a passagem do ConsumeContext para o NotificationHandler, caso exista a necessidade de utilizar o contexto, injete esta dependência onde queira utilizar.

> **É importante frizar que o `IConsumeContextAccessor` está associado diretamente ao tipo de mensagem no `INotificationHandler`, caso seja utilizado um tipo de mensagem diferente no `IConsumeContextAccessor` do tipo de mensagem no `INotificationHandler`, a propriedade `Context` será nula(Isso também vale para os `IConsumeContextAccessor` injetados nos objetos subjacentes ao `INotificationHandler`, como serviços, repositórios, etc).**

### Existem também duas classes

Uma classe contendo informações do consumo da mensagem.

```csharp
public sealed class ConsumeInformations
```

Nesta classe existem as seguintes propriedades:

- `BusName Bus`: A identificação do bus de onde a mensagem foi consumida.
- `Enum? Switch`: A __**posição**__ no __**interruptor**__ em que aquela mensagem foi consumida, o valor será nulo caso não tenha havido o uso de __**interruptores**__
- `string Endpoint`: O endpoint de onde a mensagem foi consumida.
- `IReadOnlyDictionary<string, string> Headers`: Os headers passados para o broker, caso o broker não possua o conceito de header, o dicionário será vazio.

Existe também o contexto que será passado para o middleware

```csharp
public sealed class ConsumeContext<TMessage> : ConsumeContextBase 
    where TMessage : IMessage
```

Nesta classe existem as seguintes propriedades:

- `TMessage Message`: A mensagem do tipo consumido.
- `IServiceProvider Services`: O serviçe provider para que seja utilizado para capturar as dependências.
- `ConsumeInformations`: As informações de consumo da mensagem.
- `CancellationToken CancellationToken`: O cancellation token do tempo de vida da aplicação, este só terá seu cancelamento requisitado quando a aplicação for finalizada, é importante que este cancellation token seja utilizado em tarefas que possam ser longas, como requests HTTP, acesso a bancos de dados, etc.
- `IDictionary<string, object> Items`: Um dicionário, inicialmente vazio, para que dados entre middlewares possam ser transitados.

### E um enum

```csharp
public enum BusName
{
    Kafka
}
```

Enum contendo o nome de cada bus implementado, é utilizado internamente para uma separação entre bus e exposto no ConsumeInformations.

## Monitorando o Bus

Para entender mais como monitorar o Bus afim de capturar logs e exceções, [**clique aqui**](monitoramento/monitoramento.md).

## GuimoSoft.Bus.Kafka

Projeto contendo a integração com o broker Kafka, aqui é onde haverá a _orquestração_ de quais hosts serão utilizados, quais tópicos serão associados a quais classes, quais middlewares serão executados no consumo de quais mensagens e quais serializadores serão usados para serializar e deserializar as mensagens.

Este projeto trabalha em conjunto com o [Microsoft.Extensions.DependencyInjection.Abstractions](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions) e o [Microsoft.Extensions.Hosting.Abstractions](https://github.com/aspnet/Hosting/tree/master/src/Microsoft.Extensions.Hosting.Abstractions) para integrar o bus ao host do aspnetcore.

A configuração desta etapa é flúida, utilizando builder.

### Utilizado o produtor

Existem duas etapas necessárias para a produção de mensagens

**1.** Configurar as mensagens a serem produzidas associando-as a um _endpoint_(tópico, no caso do Kafka), configurando seus servidores e seus serializadores.

Exemplo:

```csharp
services
    .AddKafkaProducer(configurer =>
    {
        configurer
            .WithDefaultSerializer(CustomDefaultSerializer.Instance) // Serializador padrão opcional
            .Produce()
                .FromType<HelloMessage>()
                .WithSerializer(HelloMessageSerializer.Instance) // Serializador por tipo opcional
                .ToEndpoint(HelloMessage.TOPIC_NAME)
            .ToServer(options =>
            {
                options.BootstrapServers = "localhost:9093";
                options.Acks = Confluent.Kafka.Acks.All;
            });
    });
```

> 1. Não é possível chamar as configurações do produtor mais de uma vez, caso isso ocorra, haverá o estouro da exceção **`BusAlreadyConfiguredException`**.
> 2. Caso não seja informada as configurações do servidor, ao se configurar o produtor (**`ToServer`**), haverá o estouro da exceção **`BusOptionsMissingException`**.
> 3. É possível registrar o mesmo tipo para mais de um endpoint(tópico do Kafka), quando a mensagem for publicada será enviada para cada um destes endpoints.
> 4. **Caso exista a necessidade de produzir mensagens para diferentes servidores, veja [_como implementar interruptores_](switches.md)**
> 5. **Caso queira entender como funcionam os serializadores e como implementá-los, veja [_Como criar serializadores_](serializadores.md)**

**2.** Utilizar o `IMessageProducer` para produzir a mensagem

Exemplo:

```csharp
private readonly IMessageProducer _producer;

[...]

await _producer.ProduceAsync(Guid.NewGuid().ToString(), new HelloMessage(name));
```

### Utilizado o consumidor

Existem, no mínimo, duas etapas que serão necessárias para o consumo de uma mensagem.

**1.** Configurar as mensagens a serem consumidas associando-as a um _endpoint_(tópico, no caso do Kafka), adicionando seus middlewares, configurando seus servidores e seus serializadores.

Exemplo:

```csharp
services
    .AddKafkaConsumer(configurer =>
    {
        configurer
            .WithDefaultSerializer(CustomDefaultSerializer.Instance) // Serializador padrão opcional
            .Consume()
                .OfType<HelloMessage>()
                .WithSerializer(HelloMessageSerializer.Instance) // Serializador por tipo opcional
                .WithMiddleware<FakePipelineMessageMiddlewareOne>(ServiceLifetime.Transient) // Middleware optional
                .FromEndpoint(HelloMessage.TOPIC_NAME)
            .FromServer(options =>
            {
                options.BootstrapServers = "google.com:9093";
                options.GroupId = "test";
            });
    })
```

> 1. Não é possível chamar as configurações do consumidor mais de uma vez, caso isso ocorra, haverá o estouro da exceção **`BusAlreadyConfiguredException`**.
> 2. Caso não seja informada as configurações do servidor, ao se configurar o consumidor (**`FromServer`**), haverá o estouro da exceção **`BusOptionsMissingException`**.
> 3. No consumo, é possível associar uma mensagem a mais de um endpoint, seja utilizando ou não, um __**interruptor**__, **caso exista a associação de mais de um tópico para um único tipo de mensagem, os serializadores serão os mesmos, não é possível realizar esta separação e o último serializador será utilizado**.
> 4. Só é possível associar um middleware a uma mensagem apenas uma vez, caso seja adicionado um middleware já configurado, este será ignorado.
> 5. **Só é possível adicionar um servidor por bus e por _posição_ no _interruptor_, caso seja tentado passar um novo servidor, o builder estoura uma exceção `ArgumentException` indicando essa possível duplicidade**.
> 6. **Caso exista a necessidade de produzir mensagens para diferentes servidores, veja [_como implementar interruptores_](switches.md)**.
> 7. **Caso queira entender como funcinam e como implementar e integrar um middleware no fluxo, entenda a pipeline [_aqui_](pipeline.md) e [aprenda a implementar e a integrar um middleware](middlewares.md)**.
> 8. **Caso queira entender como funcionam os serializadores e como implementá-los, veja [_Como criar serializadores_](serializadores.md)**.

#### *É necessário que o Handler esteja no mesmo assembly da mensagem, visto que o registro no MediatR é feito usando os assemblies das mensagens
