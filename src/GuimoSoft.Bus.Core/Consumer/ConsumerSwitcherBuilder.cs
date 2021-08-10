using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Consumer
{
    public sealed class ConsumerSwitcherBuilder<TSwitch, TOptions>
        where TSwitch : struct, Enum
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly ICollection<Assembly> _assemblies;
        private readonly IServiceCollection _services;

        private readonly List<ConsumerBuilder<TOptions>> _builders;

        internal ConsumerSwitcherBuilder(BusName busName, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _assemblies = assemblies;
            _services = services;
            _builders = new();
        }

        public ConsumerBuilder<TOptions> When(TSwitch @switch)
        {
            var builder = new ConsumerBuilder<TOptions>(_busName, @switch, _assemblies, _services);
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
