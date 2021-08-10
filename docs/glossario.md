# Glossário

Existem alguns termos que podem parecer estranhos, visto que existem alguns termos que foram criados afim de abstrair alguns conceitos que possuem diferentes nomes entre os brokers.

1. **Pipeline**: Implementação do padrão comportamental [**chain of responsibility**](https://medium.com/xp-inc/design-patterns-parte-15-chain-of-resposability-8790ebb5d443), é responsável, dentro do fluxo de consumo de uma mensagem no Bus, por facilitar a adição de novas funcionalidades dentro desse consumo, para saber mais, [_**clique aqui**_](pipeline.md).

2. **Middleware**: Um componente a ser adicionado dentro da pipeline, sua finalidade depende do que se espera dele, é possível criar um middleware como um filtro(pode parar o fluxo de execução), um capturador de exceções, uma ferramenta de captura de tempo de execução para monitorias ou como uma ferramenta para rotear a continuação do fluxo entre diferentes inquilinos, para saber mais, [_**clique aqui**_](middlewares.md).

3. **Handler**: Á última etapa da execução da pipeline, é uma implementação da interface `INotificationHandler` do [MediatR](https://github.com/jbogard/MediatR).

4. **Serializador**: Implementações responsáveis por converter objetos em array de bytes, são usadas para, no consumo, converter a mensagem do broker no objeto associado aquele endpoint e para, na produção, converter o objeto na mensagem a ser enviada pelo broker, para saber mais, [_**clique aqui**_](serializadores.md).

5. **Endpoint**: Identificação do local, no broker, onde a mensagem será consumida ou produzida, é uma abstração de termos como ***Tópico**, no Kafka, e **Fila**, no RabbitMQ.

6. **Interruptor**: É um `enum` que tem por finalidade possibilitar a implementação de consumidores e produtores de um mesmo broker a mais de um servidor ou de uma mensagem a mais de um tópico, para saber mais, [_**clique aqui**_](switches.md).

7. **Posição**: É um item do `enum` do **interruptor**, ele representa uma "partição" diferente dentro de um broker, conforme explicado no **interruptor**, cada uma dessas posições possui suas próprias configurações, para saber mais, [_**clique aqui**_](switches.md).
