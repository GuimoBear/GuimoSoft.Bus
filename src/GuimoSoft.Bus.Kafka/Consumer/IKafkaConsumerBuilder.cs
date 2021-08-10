using Confluent.Kafka;
using System;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaConsumerBuilder
    {
        IConsumer<string, byte[]> Build(Enum @switch);
    }
}