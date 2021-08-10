using System;
using System.Threading;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaTopicMessageConsumer
    {
        void ConsumeUntilCancellationIsRequested(Enum @switch, string topic, CancellationToken cancellationToken);
    }
}