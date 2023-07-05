# Interruptores

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Por padrão, a implementação do bus trabalha com um único servidor do broker para cada finalidade(consumir e produzir).

Existem situações em que é necessário que cada finalidade consiga trabalhar com mais de um servidor, para isso, foi criado o conceito de __**interruptor**__.

## Afinal, o que é um interruptor?

Um __**interruptor**__ é, simplesmente, uma forma de _rotear_ os fluxos de consumo e produção por mais de um caminho, de acordo com uma __**posição**__ neste __**interruptor**__.

As posições do interruptor são, em suma, representadas por um **`enum`**.

## Para que serve?  

Implementar __**interruptores**__ trás as seguintes possibilidades:

1. Utilizar mais de um servidor tanto no consumidor quanto no produtor.
2. Implementar o consumo de uma mesma mensagem de diferentes formas(ajustando os middlewares, os serializadores, etc).
3. Implementar a produção de uma mesma mensagem para diferentes servidores ou para diferentes endpoints.

## Implementando

Exemplo:

```csharp
public enum ServerName
{
    Host1,
    Host2
}

[...]

services
    .AddKafkaConsumerSwitcher<ServerName>(switcher =>
    {
        switcher
            .When(ServerName.Host1)
                .Consume()
                    .OfType<HelloMessage>()
                    .FromEndpoint(HelloMessage.TOPIC_NAME)
                .FromServer(options =>
                {
                    options.BootstrapServers = "localhost:9093";
                    options.GroupId = "test";
                });

        switcher
            .When(ServerName.Host2)
                .Consume()
                    .OfType<HelloMessage>()
                    .WithSerializer(HelloMessageSerializer.Instance) // Serializador opcional
                    .WithMiddleware<FakePipelineMessageMiddlewareOne>(ServiceLifetime.Transient) // Middleware optional
                    .FromEndpoint(HelloMessage.TOPIC_NAME)
                .FromServer(options =>
                {
                    options.BootstrapServers = "google.com:9093";
                    options.GroupId = "test";
                });

    })
    .AddKafkaProducerSwitcher<ServerName>(switcher =>
    {
        switcher
            .When(ServerName.Host1)
                .Produce()
                    .FromType<HelloMessage>()
                    .ToEndpoint(HelloMessage.TOPIC_NAME)
                .ToServer(options =>
                {
                    options.BootstrapServers = "localhost:9093";
                    options.Acks = Confluent.Kafka.Acks.All;
                });
        switcher
            .When(ServerName.Host2)
                .Produce()
                    .FromType<HelloMessage>()
                    .ToEndpoint(HelloMessage.TOPIC_NAME)
                .ToServer(options =>
                {
                    options.BootstrapServers = "google.com:9093";
                    options.Acks = Confluent.Kafka.Acks.All;
                });
    });
```

> 1. Para esta situação, é possível que a mesma mensagem possa ser configurada para cada _**posição**_ deste _**interruptor**_(o consumo da mesma mensagem, com diferentes middlewares, diferentes serializadores para cada host, por exemplo), sobrepondo a regra da configuração sem _**interruptor**_.
> 2. Assim como a configuração do produtor mostrada anteriormente, também é possível configurar uma mensagem para ser enviada para mais de um endpoint e, também, para mais de um host.
> 3. Não é possível chamar as configurações do consumidor ou do produtor para a mesma _**posição**_ do _**interruptor**_ mais de uma vez, caso isso ocorra, haverá o estouro da exceção **`BusAlreadyConfiguredException`**.
> 4. Caso não seja informada as configurações do servidor, seja do consumidor ou do produtor, para qualquer _**posição**_ do _**interruptor**_, haverá o estouro da exceção **`BusOptionsMissingException`**.
> 5. É possível chamar os métodos `AddKafkaConsumerSwitcher` ou `AddKafkaProducerSwitcher` mais de uma vez, contanto que a _**posição**_ no _**interruptor**_ não tenha sido anteriormente configurada.
