# Changelog

All notable changes to this [project](README.md) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Não publicado]

### Adicionado

- Criação da documentação do projeto de [**_Bus_**](./docs/bus/README.md)
- Implementação para tornar a configuração das mensagens no bus flúida.
- Criação de __**interruptores**__, usados para trabalhar com mais de um servidor por bus.
- Passagem do CancellationToken de lifetime da aplicação para a Pipeline e para o Mediator.
- Criação do ConsumeInformations contendo informações do Bus, do __**interruptor**__ e do __**endpoint**__, passado dentro do ConsumeContext.
- Implementação dos eventos `BusExceptionMessage` e `BusTypedExceptionMessage<TMessage>` para a captura, utilizando o `INotificationHandler` do [MediatR](https://github.com/jbogard/MediatR), de exceções ocorridas no Bus, para saber mais [**clique aqui**](docs/bus/monitoramento/monitoramento.md) ou [**aqui**](docs/bus/monitoramento/excecoes.md).
- Implementação dos eventos `BusLogMessage` e `BusTypedLogMessage<TMessage>` para a captura, utilizando o `INotificationHandler` do [MediatR](https://github.com/jbogard/MediatR), de logs enviados pelo Bus, para saber mais [**clique aqui**](docs/bus/monitoramento/monitoramento.md) ou [**aqui**](docs/bus/monitoramento/logs.md).

### Modificado

- Melhoria na performance da pipeline usando Emit(criação de código IL em tempo de execução).
- Movidas as implementações de Serialização, dicionário de endpoints(tópicos do Kafka) e roteador de publicação para o projeto Core do Bus afim de tornar possível o consumo e a produção de mais de um host de um broker no mesmo projeto e facilitar a implementação de novos brokers.
- Renomeação do ConsumeContext para ConsumeContext
- Utilizar as configurações de consumo e produção nativas da LIB Confluent.Kafka
- Tratamento para estourar uma exceção quando houver uma tentativa de adicionar mais de uma configuração de bus para um mesmo [**_Bus_**](./docs/bus/README.md), e __**posição**__ no __**interruptor**__.
- (Fail fast) Implementação de validações das configurações do Bus no momento da injeção de dependência

## [1.0.3] - 13-07-2021

### Adicionado

- Publicação inicial das libs
