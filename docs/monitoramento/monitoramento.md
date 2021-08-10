# Logs

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Monitorar o Bus é uma tarefa simples, necessitanto de pouca implementação para começar a ocorrer.

É importante termos noção dos níveis de logs que o Bus está enviando, estes logs estão representados no enum `BusLogLevel`.

## Log level

As exceções se enquadram em 6 categorias diferentes e estão descritos no enum **`BusLogLevel`**:

1. `BusLogLevel.Trace`: Evento de rastreamento.
2. `BusLogLevel.Debug`: Evento de depuração.
3. `BusLogLevel.Information`: Evento de informação.
4. `BusLogLevel.Warning`: Evento de atenção.
5. `BusLogLevel.Error`: Evento de erro.
6. `BusLogLevel.Critical`: Evento crítico.

> O **`BusLogLevel`** possui os mesmos valores do [**`LogLevel`**](https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging.Abstractions/LogLevel.cs) da [**`Microsoft.Extensions.Logging.Abstractions`**](https://github.com/aspnet/Logging/tree/master/src/Microsoft.Extensions.Logging.Abstractions)

A captura de logs será feita, assim como os handlers das mensagens, dependem do `INotificationHandler` do [MediatR](https://github.com/jbogard/MediatR).

## Entendendo os logs

1. Para saber mais sobre os **logs gerais**, [**clique aqui**](logs.md);
2. Para saber mais sobre os **logs de exceções**, [**clique aqui**](excecoes.md);

### * É necessário que, assim como os Handlers das mensagens, os handlers das exceções e dos logs DEVEM estar nos mesmos assemblies das mensagens
