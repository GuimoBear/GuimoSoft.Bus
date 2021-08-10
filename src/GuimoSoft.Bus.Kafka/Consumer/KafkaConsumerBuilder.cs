using Confluent.Kafka;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal class KafkaConsumerBuilder : ClientBuilder, IKafkaConsumerBuilder
    {
        private readonly IBusOptionsDictionary<ConsumerConfig> _options;

        public KafkaConsumerBuilder(IBusOptionsDictionary<ConsumerConfig> options, IBusLogDispatcher logger)
            : base(logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IConsumer<string, byte[]> Build(Enum @switch)
        {
            if (!_options.TryGetValue(@switch, out var consumerConfig))
                throw new KeyNotFoundException($"N�o existe uma configura��o do Kafka para o server {@switch}");

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(consumerConfig);

            consumerBuilder.SetLogHandler((_, kafkaLogMessage) => LogMessage(@switch, Finality.Consume, kafkaLogMessage));
            consumerBuilder.SetErrorHandler((_, kafkaError) => LogException(@switch, Finality.Consume, kafkaError));

            return consumerBuilder.Build();
        }
    }
}