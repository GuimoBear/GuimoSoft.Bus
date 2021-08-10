using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Core.Producer
{
    public class TypedEndpointProducerBuilder<TOptions, TEvent>
        where TOptions : class, new()
        where TEvent : IEvent
    {
        private readonly ProducerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly MessageTypeCache _messageTypesCache;

        internal TypedEndpointProducerBuilder(
            ProducerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            MessageTypeCache messageTypesCache)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _messageTypesCache = messageTypesCache;
        }

        public TypedEndpointProducerBuilder<TOptions, TEvent> WithSerializer(TypedSerializer<TEvent> typedSerializer)
        {
            _busSerializerManager.AddTypedSerializer(_busName, Finality.Produce, _switch, typedSerializer);

            return this;
        }

        public ProducerBuilder<TOptions> ToEndpoint(string endpoint)
        {
            _messageTypesCache.Add(_busName, Finality.Produce, _switch, typeof(TEvent), endpoint);
            return _parent;
        }
    }
}
