using Confluent.Kafka;
using System;

namespace GuimoSoft.Bus.Kafka.Producer
{
    public interface IKafkaProducerBuilder
    {
        IProducer<string, byte[]> Build(Enum @switch);
    }
}