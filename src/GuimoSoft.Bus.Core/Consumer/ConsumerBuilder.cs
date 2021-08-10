using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core.Consumer
{
    public sealed class ConsumerBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly ICollection<Assembly> _assemblies;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventMiddlewareManager _middlewareManager;
        private readonly MessageTypeCache _messageTypesCache;
        private readonly BusOptionsDictionary<TOptions> _optionsDictionary;

        internal ConsumerBuilder(BusName busName, Enum @switch, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _switch = @switch;
            _assemblies = assemblies;

            _busSerializerManager = Singletons.TryRegisterAndGetBusSerializerManager(services);
            _middlewareManager = Singletons.TryRegisterAndGetMessageMiddlewareManager(services);
            _messageTypesCache = Singletons.TryRegisterAndGetMessageTypeCache(services);
            _optionsDictionary = Singletons.TryRegisterAndGetBusOptionsDictionary<TOptions>(services);
            ValidateBusOptions();
        }

        public ConsumerBuilder<TOptions> FromServer(Action<TOptions> configure)
        {
            var config = new TOptions();
            configure(config);
            _optionsDictionary[_switch] = config;
            return this;
        }

        public ConsumerBuilder<TOptions> WithDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            _busSerializerManager.SetDefaultSerializer(_busName, Finality.Consume, _switch, defaultSerializer);

            return this;
        }

        public EndpointConsumerBuilder<TOptions> Consume()
            => new EndpointConsumerBuilder<TOptions>(this, _busName, _switch, _busSerializerManager, _middlewareManager, _messageTypesCache, _assemblies);

        internal void ValidateAfterConfigured()
        {
            if (!_optionsDictionary.ContainsKey(_switch))
                throw new BusOptionsMissingException(_busName, _switch, typeof(TOptions));
        }

        private void ValidateBusOptions()
        {
            if (_optionsDictionary.ContainsKey(_switch))
                throw new BusAlreadyConfiguredException(_busName, _switch);
        }
    }
}
