using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Producer
{
    public class EndpointProducerBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly ProducerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly MessageTypeCache _messageTypesCache;
        private readonly ICollection<Assembly> _assemblies;

        internal EndpointProducerBuilder(
            ProducerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            MessageTypeCache messageTypesCache,
            ICollection<Assembly> assemblies)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _messageTypesCache = messageTypesCache;
            _assemblies = assemblies;
        }

        public TypedEndpointProducerBuilder<TOptions, TEvent> FromType<TEvent>() where TEvent : IEvent
        {
            _assemblies.TryAddAssembly(typeof(TEvent).Assembly);
            return new TypedEndpointProducerBuilder<TOptions, TEvent>(_parent, _busName, _switch, _busSerializerManager, _messageTypesCache);
        }
    }
}
