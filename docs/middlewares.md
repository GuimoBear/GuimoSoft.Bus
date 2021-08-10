# Middlewares

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Assim como um middleware no [ASP.NET Core](https://github.com/dotnet/aspnetcore), um middleware no Bus é chamado entre o consumo da mensagem na fila e seu envio para o handler.

Utilizando middleware, é possível:  

1. Ajustar valores dentro do escopo da chamada(para aplicações multiinquilinos, por exemplo).

2. Capturar os erros que ocorrerão nos middlewares subjacentes.

3. Interromper o fluxo da pipeline, fazendo com que os middlewares subjacentes e o handler não sejam executados.

## Implementando um middleware

Para a criação de um middleware, é necessário implementarmos a interface `IMessageMiddleware<TMessage>`:

```csharp
public class HelloMessageMiddleware : IMessageMiddleware<HelloMessage>
{
    private readonly ILogger<HelloMessageMiddleware> _logger;

    public HelloMessageMiddleware(ILogger<HelloMessageMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(ConsumeContext<HelloMessage> context, Func<Task> next)
    {
        //Código executado antes dos pipelines subjacentes e do handler
        _logger.Debug($"Middleware {nameof(HelloMessageMiddleware)} iniciado");
        await next();
        _logger.Debug($"Middleware {nameof(HelloMessageMiddleware)} finalizado");
        //Código executado após os pipelines subjacentes e do handler
    }
}
```

> 1. Caso o método `next()` não seja chamado, o fluxo da pipeline será interrompido.
> 2. Se houver qualquer exceção não tratada nos middlewares subjacentes, esta chegará até o middleware atual.

## Registrando um middleware

O registro dos middlewares é feito junto ao registro das mensagens.

### Forma padrão

```csharp
.Consume()
    .OfType<HelloMessage>()
    // Middleware criado sempre que é requisitado
    .WithMiddleware<FirstHelloMessageMiddleware>(ServiceLifetime.Transient)
    // Middleware criado usando um método favctory uma vez durante o escopo da requisição
    .WithMiddleware(prov => new SecondHelloMessageMiddleware(), ServiceLifetime.Scoped) 
    // Middleweare criado uma vez no lifetime da aplicação.
    .WithMiddleware<ThirdHelloMessageMiddleware>(ServiceLifetime.Singleton)
    // Middleware criado como Singleton(assim como o anterior)
    .WithMiddleware<FourthHelloMessageMiddleware>() 
    .FromEndpoint(HelloMessage.TOPIC_NAME)
```

> 1. Assim como os middlewares do Asp.NET Core, a ordem de registro de um middleware afeta a ordem de sua execução, não se atentar nesta ordem pode ocasionar quebras no fluxo de execução.
> 2. O Lifetime Scoped ou transient, no caso do middleware, terão o mesmo comportamento, visto que a criação de um middleware é requerida apenas uma vez durante o escomo do recebimento de uma mensagem.

### Utilizando interruptores

Para o caso da implementação de __**interruptores**__, Os middlewares são separados por tipo de mensagem e por __**posição**__ no __**interruptor**__:

```csharp
public enum ServerName
{
    Host1,
    Host2
}

[...]
switcher
    .When(ServerName.Host1)
        .Consume()
            .OfType<HelloMessage>()
            .WithMiddleware<FirstHelloMessageMiddleware>(ServiceLifetime.Transient)
            .WithMiddleware(prov => new SecondHelloMessageMiddleware(), ServiceLifetime.Scoped) 
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
            .WithMiddleware<ThirdHelloMessageMiddleware>(ServiceLifetime.Singleton)
            .WithMiddleware<FourthHelloMessageMiddleware>() 
            .FromEndpoint(HelloMessage.TOPIC_NAME)
        .FromServer(options =>
        {
            options.BootstrapServers = "google.com:9093";
            options.GroupId = "test";
        });
```

- As mensagens recebidas no `ServerName.Host1` executarão apenas os middlewares `FirstHelloMessageMiddleware` e `SecondHelloMessageMiddleware`, necessariamente nesta ordem.
- As mensagens recebidas no `ServerName.Host2` executarão apenas os middlewares `ThirdHelloMessageMiddleware` e `FourthHelloMessageMiddleware`, necessariamente nesta ordem.
