using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Producer
{
    public sealed class ProducerSwitcherBuilder<TSwitch, TOptions>
        where TSwitch : struct, Enum
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly ICollection<Assembly> _assemblies;
        private readonly IServiceCollection _services;

        private readonly List<ProducerBuilder<TOptions>> _builders;

        internal ProducerSwitcherBuilder(BusName busName, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _assemblies = assemblies;
            _services = services;
            _builders = new();
        }

        public ProducerBuilder<TOptions> When(TSwitch @switch)
        {
            var builder = new ProducerBuilder<TOptions>(_busName, @switch, _assemblies, _services);
            _builders.Add(builder);
            return builder;
        }

        internal void ValidateAfterConfigured()
        {
            foreach (var builder in _builders)
                builder.ValidateAfterConfigured();
        }
    }
}
