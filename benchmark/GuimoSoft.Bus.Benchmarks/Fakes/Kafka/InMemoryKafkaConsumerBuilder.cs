using Confluent.Kafka;
using System;
using GuimoSoft.Bus.Benchmarks.Fakes.Interfaces;
using GuimoSoft.Bus.Kafka.Consumer;

namespace GuimoSoft.Bus.Benchmarks.Fakes.Kafka
{
    public class InMemoryKafkaConsumerBuilder : IKafkaConsumerBuilder
    {
        private readonly IBrokerConsumer _consumer;

        public InMemoryKafkaConsumerBuilder(IBrokerConsumer consumer)
        {
            _consumer = consumer;
        }

        public IConsumer<string, byte[]> Build(Enum @switch)
            => new InMemoryConsumer(_consumer);
    }
}
