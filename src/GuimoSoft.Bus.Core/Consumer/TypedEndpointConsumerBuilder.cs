using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Core.Consumer
{
    public class TypedEndpointConsumerBuilder<TOptions, TEvent>
        where TOptions : class, new()
        where TEvent : IEvent
    {
        private readonly ConsumerBuilder<TOptions> _parent;
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventMiddlewareManager _middlewareManager;
        private readonly MessageTypeCache _messageTypesCache;

        internal TypedEndpointConsumerBuilder(
            ConsumerBuilder<TOptions> parent,
            BusName busName,
            Enum @switch,
            BusSerializerManager busSerializerManager,
            EventMiddlewareManager middlewareManager,
            MessageTypeCache messageTypesCache)
        {
            _parent = parent;
            _busName = busName;
            _switch = @switch;

            _busSerializerManager = busSerializerManager;
            _middlewareManager = middlewareManager;
            _messageTypesCache = messageTypesCache;
        }

        public TypedEndpointConsumerBuilder<TOptions, TEvent> WithSerializer(TypedSerializer<TEvent> typedSerializer)
        {
            _busSerializerManager.AddTypedSerializer(_busName, Finality.Consume, _switch, typedSerializer);

            return this;
        }

        public TypedEndpointConsumerBuilder<TOptions, TEvent> WithMiddleware<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            _middlewareManager.Register<TEvent, TMiddleware>(BusName.Kafka, _switch, lifetime);
            return this;
        }

        public TypedEndpointConsumerBuilder<TOptions, TEvent> WithMiddleware<TMiddleware>(Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TMiddleware : class, IEventMiddleware<TEvent>
        {
            _middlewareManager.Register<TEvent, TMiddleware>(BusName.Kafka, _switch, factory, lifetime);
            return this;
        }

        public ConsumerBuilder<TOptions> FromEndpoint(string endpoint)
        {
            _messageTypesCache.Add(_busName, Finality.Consume, _switch, typeof(TEvent), endpoint);
            return _parent;
        }
    }
}
