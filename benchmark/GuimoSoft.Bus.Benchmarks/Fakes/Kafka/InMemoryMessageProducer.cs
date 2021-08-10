using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Benchmarks.Fakes.Interfaces;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Benchmarks.Fakes.Kafka
{
    public class InMemoryMessageProducer : IEventDispatcher
    {
        private readonly IBrokerProducer _producer;

        public InMemoryMessageProducer(IBrokerProducer producer)
        {
            _producer = producer;
        }

        public Task DispatchAsync<TEvent>(string key, TEvent message, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            _producer.Enqueue(key, JsonMessageSerializer.Instance.Serialize(message));
            return Task.CompletedTask;
        }
    }
}
