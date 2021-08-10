using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class ProducerManager : IProducerManager
    {
        private readonly object _lock = new();
        private readonly IServiceCollection _services;

        private readonly ICollection<(BusName Bus, Type ProducerType)> _producerTypes;

        public ProducerManager(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _producerTypes = new List<(BusName Bus, Type ProducerType)>();
        }

        public void Add<BusProducer>(BusName busName) where BusProducer : class, IBusEventDispatcher
        {
            lock (_lock)
            {
                var producerBype = typeof(BusProducer);
                var producerType = (busName, producerBype);
                if (!_producerTypes.Any(x => x.Bus.Equals(busName)))
                {
                    _producerTypes.Add(producerType);
                    _services.TryAddTransient(producerBype);
                }
            }
        }

        public IBusEventDispatcher GetProducer(BusName busName, IServiceProvider services)
        {
            var producerType = _producerTypes.First(x => x.Bus == busName).ProducerType;
            return services.GetRequiredService(producerType) as IBusEventDispatcher;
        }
    }
}
