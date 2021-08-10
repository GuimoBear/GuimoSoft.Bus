using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class MessageProducer : IEventDispatcher
    {
        private readonly IProducerManager _producerManager;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly IServiceProvider _services;

        public MessageProducer(IProducerManager producerManager, IMessageTypeCache messageTypeCache, IServiceProvider services)
        {
            _producerManager = producerManager ?? throw new ArgumentNullException(nameof(producerManager));
            _messageTypeCache = messageTypeCache ?? throw new ArgumentNullException(nameof(messageTypeCache));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task DispatchAsync<TEvent>(string key, TEvent message, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            ValidateParameters(key, message);
            foreach (var (busName, @switch, endpoint) in _messageTypeCache.Get(message.GetType()))
            {
                await _producerManager
                    .GetProducer(busName, _services)
                    .DispatchAsync(key, message, @switch, endpoint, cancellationToken);
            }
        }

        private static void ValidateParameters<TEvent>(string key, TEvent message) where TEvent : IEvent
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("É necessário informar uma chave para enviar a mensagem", nameof(key));
        }
    }
}
