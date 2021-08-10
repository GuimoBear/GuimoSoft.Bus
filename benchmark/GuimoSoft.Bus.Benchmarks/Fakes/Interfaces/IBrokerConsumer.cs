using System;
using System.Collections.Generic;
using System.Threading;

namespace GuimoSoft.Bus.Benchmarks.Fakes.Interfaces
{
    public interface IBrokerConsumer
    {
        void CreateTopicIfNotExists(string topic);

        (string topic, byte[] message) Consume(IEnumerable<string> topics, int millisecondsTimeout);

        (string topic, byte[] message) Consume(IEnumerable<string> topics, CancellationToken cancellationToken);

        (string topic, byte[] message) Consume(IEnumerable<string> topics, TimeSpan timeout);
    }
}
