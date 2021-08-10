using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GuimoSoft.Bus.Benchmarks.Fakes.Interfaces;

namespace GuimoSoft.Bus.Benchmarks.Fakes.Kafka
{
    public sealed class InMemoryBroker : IBrokerProducer, IBrokerConsumer
    {
        private static readonly InMemoryBroker instance = new InMemoryBroker();

        public static IBrokerProducer Producer => instance;

        public static IBrokerConsumer Consumer => instance;

        private readonly ConcurrentDictionary<string, Queue<byte[]>> queues = new ();

        private InMemoryBroker() { }

        public void Enqueue(string topic, byte[] message)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (queues.TryGetValue(topic, out var queue))
                queue.Enqueue(message);
            else
            {
                queue = new();
                queue.Enqueue(message);
                queues.TryAdd(topic, queue);
            }
        }

        public void CreateTopicIfNotExists(string topic)
        {
            if (!queues.TryGetValue(topic, out _))
            {
                var queue = new Queue<byte[]>();
                queues.TryAdd(topic, queue);
            }
        }

        public (string topic, byte[] message) Consume(IEnumerable<string> topics, TimeSpan timeout)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            var timeoutDateTime = DateTime.Now.Add(timeout);
            while (timeoutDateTime > DateTime.Now)
            {
                foreach (var topic in topics)
                    if (queues.TryGetValue(topic, out var queue) && queue.Count > 0)
                        return (topic, queue.Dequeue());
            }
            return default;
        }

        public (string topic, byte[] message) Consume(IEnumerable<string> topics, int millisecondsTimeout)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            return Consume(topics, TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public (string topic, byte[] message) Consume(IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            if (cancellationToken == default)
                return Consume(topics, TimeSpan.FromSeconds(20));
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var topic in topics)
                    if (queues.TryGetValue(topic, out var queue) && queue.Count > 0)
                        return (topic, queue.Dequeue());
            }
            return default;
        }
    }
}
