using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Consumer
{
    public class EndpointConsumerBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly ConsumerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventMiddlewareManager _middlewareManager;
        private readonly MessageTypeCache _messageTypesCache;
        private readonly ICollection<Assembly> _assemblies;

        internal EndpointConsumerBuilder(
            ConsumerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventMiddlewareManager middlewareManager,
            MessageTypeCache messageTypesCache,
            ICollection<Assembly> assemblies)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _middlewareManager = middlewareManager;
            _messageTypesCache = messageTypesCache;
            _assemblies = assemblies;
        }

        public TypedEndpointConsumerBuilder<TOptions, TEvent> OfType<TEvent>() where TEvent : IEvent
        {
            _assemblies.TryAddAssembly(typeof(TEvent).Assembly);
            return new TypedEndpointConsumerBuilder<TOptions, TEvent>(_parent, _busName, _switch, _busSerializerManager, _middlewareManager, _messageTypesCache);
        }
    }
}
