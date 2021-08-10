# Serializadores

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Por padrão, as mensagens utilizam a lib [System.Text.Json](https://docs.microsoft.com/pt-br/dotnet/api/system.text.json?view=net-5.0) e `Encoding.UTF8`.

## Implementação

Porém, caso haja a necessidade de implementação de um serializador diferente, existem duas formas:

### 1. Implementar a serialização padrão implementando a interface `IDefaultSerializer`

```csharp
public sealed class JsonMessageSerializer : IDefaultSerializer
{
    // Serializadores, para o Bus, sempre será singleton
    public static readonly JsonMessageSerializer Instance = new();

    // construtor privado para garantir que exista apenas uma instáncia deste serializador
    private JsonMessageSerializer() { }

    public byte[] Serialize(object message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message);
    }

    public object Deserialize(Type messageType, byte[] content)
    {
        var stringContent = Encoding.UTF8.GetString(content);
        return JsonSerializer.Deserialize(stringContent, messageType);
    }
}
```

> Serializadores, na implementação do Bus, necessariamente serão singletons.

### 2. Implementar uma serialização para um tipo específico herdando da classe abstrata `TypedSerializer<TType>`

```csharp
public class HelloMessageSerializer : TypedSerializer<HelloMessage>
{
    // Serializadores, para o Bus, sempre será singleton
    public static HelloMessageSerializer Instance = new();

    // construtor privado para garantir que exista apenas uma instáncia deste serializador
    private HelloMessageSerializer() { }

    protected override HelloMessage Deserialize(byte[] content)
    {
        return JsonSerializer.Deserialize<HelloMessage>(Encoding.UTF8.GetString(content));
    }

    protected override byte[] Serialize(HelloMessage message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message);
    }
}
```

> A classe abstrata implementa, por trás dos panos, a interface `IDefaultSerializer`

## Registro

O registro dos serializadores é feito junto ao registro das mensagens.

```csharp
// Registro dos serializadores nos consumidores
configurer
    .WithDefaultSerializer(CustomDefaultSerializer.Instance) // O registro de um serializador padrão
    .Consume()
        .OfType<HelloMessage>()
        .WithSerializer(HelloMessageSerializer.Instance) // O registro de um serializador para um tipo específico
        .FromEndpoint(HelloMessage.TOPIC_NAME)
    .FromServer(options =>
    {
        options.BootstrapServers = "google.com:9093";
        options.GroupId = "test";
    });

[...]

// Registro dos serializadores nos produtores
configurer
    .WithDefaultSerializer(CustomDefaultSerializer.Instance) // O registro de um serializador padrão
    .Produce()
        .OfType<HelloMessage>()
        .WithSerializer(HelloMessageSerializer.Instance) // O registro de um serializador para um tipo específico
        .ToEndpoint(HelloMessage.TOPIC_NAME)
    .ToServer(options =>
    {
        options.BootstrapServers = "localhost:9093";
        options.Acks = Confluent.Kafka.Acks.All;
    });
```

> 1. O registro de um serializados específico para um tipo sobrepões o serializador padrão apenas para este tipo dentro deste fluxo.
> 2. Os serializadores serão registrados usando a finalizade(produzir e consumir) e a __**posição**__(caso tenha sido implementado o __**interruptor**__) como chave, com isso, podem haver serializadores padrão para cada finalizade e para cada __**posição**__.
