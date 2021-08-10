using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core.Producer
{
    public sealed class ProducerBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly ICollection<Assembly> _assemblies;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly MessageTypeCache _messageTypesCache;
        private readonly BusOptionsDictionary<TOptions> _optionsDictionary;

        internal ProducerBuilder(BusName busName, Enum @switch, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _switch = @switch;
            _assemblies = assemblies;

            _busSerializerManager = Singletons.TryRegisterAndGetBusSerializerManager(services);
            _messageTypesCache = Singletons.TryRegisterAndGetMessageTypeCache(services);
            _optionsDictionary = Singletons.TryRegisterAndGetBusOptionsDictionary<TOptions>(services);
            ValidateBusOptions();
        }

        public ProducerBuilder<TOptions> ToServer(Action<TOptions> configure)
        {
            var config = new TOptions();
            configure(config);
            _optionsDictionary[_switch] = config;
            return this;
        }

        public ProducerBuilder<TOptions> WithDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            _busSerializerManager.SetDefaultSerializer(_busName, Finality.Produce, _switch, defaultSerializer);

            return this;
        }

        public EndpointProducerBuilder<TOptions> Produce()
            => new EndpointProducerBuilder<TOptions>(this, _busName, _switch, _busSerializerManager, _messageTypesCache, _assemblies);

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
