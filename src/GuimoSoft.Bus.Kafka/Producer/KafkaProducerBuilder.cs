using Confluent.Kafka;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Kafka.Producer
{
    internal class KafkaProducerBuilder : ClientBuilder, IKafkaProducerBuilder
    {
        private readonly IBusOptionsDictionary<ProducerConfig> _options;

        public KafkaProducerBuilder(IBusOptionsDictionary<ProducerConfig> options, IBusLogDispatcher logger)
            : base(logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IProducer<string, byte[]> Build(Enum @switch)
        {
            if (!_options.TryGetValue(@switch, out var ProducerConfig))
                throw new KeyNotFoundException($"N�o existe uma configura��o do Kafka para o server {@switch}");

            var producerBuilder = new ProducerBuilder<string, byte[]>(ProducerConfig);

            producerBuilder.SetLogHandler((_, kafkaLogMessage) => LogMessage(@switch, Finality.Produce, kafkaLogMessage));
            producerBuilder.SetErrorHandler((_, kafkaError) => LogException(@switch, Finality.Produce, kafkaError));

            return producerBuilder.Build();
        }
    }
}