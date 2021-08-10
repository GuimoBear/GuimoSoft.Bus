using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class MessageTypeCache : IMessageTypeCache
    {
        private readonly object _lock = new();

        private readonly ICollection<MessageTypeItem> _messageTypes;

        public MessageTypeCache()
        {
            _messageTypes = new List<MessageTypeItem>();
        }

        public void Add(BusName busName, Finality finality, Enum @switch, Type type, string endpoint)
        {
            if (!typeof(IEvent).IsAssignableFrom(type))
                throw new ArgumentException($"{type.Name} deve implementar a interface {nameof(IEvent)}");
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("É necessário informar um endpoint para haver o registro", nameof(endpoint));

            var item = new MessageTypeItem(busName, finality, @switch, type, endpoint);
            lock (_lock)
            {
                if (!_messageTypes.Contains(item))
                    _messageTypes.Add(item);
            }
        }

        public IEnumerable<Enum> GetSwitchers(BusName busName, Finality finality)
        {
            var items = _messageTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality))
                .Select(mt => mt.Switch)
                .Distinct()
                .ToList();
            if (items.Count == 0)
                throw new InvalidOperationException($"Não existem switches para o bus {busName}");
            return items;
        }

        public IEnumerable<string> GetEndpoints(BusName busName, Finality finality, Enum @switch)
        {
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            var items = _messageTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch))
                .Select(mt => mt.Endpoint)
                .ToList();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, IEvent message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            var messageType = message.GetType();

            return Get(busName, finality, @switch, messageType);
        }

        public IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, Type messageType)
        {
            if (messageType is null)
                throw new ArgumentNullException(nameof(messageType));
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));

            var items = _messageTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch) && mt.Type.Equals(messageType))
                .Select(mt => mt.Endpoint)
                .ToList();
            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IReadOnlyCollection<Type> Get(BusName busName, Finality finality, Enum @switch, string endpoint)
        {
            if (@switch is null)
                throw new ArgumentNullException(nameof(@switch));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("É necessário informar um endpoint obter os tipos da mensagem", nameof(endpoint));

            var items = _messageTypes
                .Where(mt => mt.Bus.Equals(busName) && mt.Finality.Equals(finality) && mt.Switch.Equals(@switch) && mt.Endpoint.Equals(endpoint))
                .Select(mt => mt.Type)
                .ToList();
            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para o bus {busName} e para o switch {@switch}");
            return items;
        }

        public IEnumerable<(BusName BusName, Enum Switch, string Endpoint)> Get(Type messageType)
        {
            if (messageType is null)
                throw new ArgumentNullException(nameof(messageType));

            var items = _messageTypes
                   .Where(mt => mt.Finality.Equals(Finality.Produce) && mt.Type.Equals(messageType))
                   .Select(mt => (mt.Bus, mt.Switch, mt.Endpoint))
                   .ToList();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Não existem endpoints configurados para a mensagem do tipo {messageType}");
            return items;
        }

        internal sealed class MessageTypeItem
        {
            public BusName Bus { get; }
            public Finality Finality { get; }
            public Enum Switch { get; }
            public Type Type { get; }
            public string Endpoint { get; }

            private readonly int _hashCode;

            public MessageTypeItem(BusName bus, Finality finality, Enum @switch, Type type, string endpoint)
            {
                Bus = bus;
                Finality = finality;
                Switch = @switch;
                Type = type;
                Endpoint = endpoint;

                _hashCode = HashCode.Combine(Bus, Finality, Switch, Type, Endpoint);
            }

            public override bool Equals(object obj)
            {
                return obj is MessageTypeItem item &&
                       item.GetHashCode().Equals(GetHashCode());
            }

            public override int GetHashCode()
                => _hashCode;
        }
    }
}
