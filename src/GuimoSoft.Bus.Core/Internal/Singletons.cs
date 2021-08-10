using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal static class Singletons
    {
        public static readonly object _lock = new();

        private static BusSerializerManager _lazyBusSerializerManagerSingleton { get; set; } = default;
        private static EventMiddlewareManager _lazyMessageMiddlewareManagerSingleton { get; set; } = default;
        private static MessageTypeCache _lazyMessageTypeCacheSingleton { get; set; } = default;
        private static ProducerManager _lazyProducerManagerSingleton { get; set; } = default;
        private static ConcurrentDictionary<Type, object> _busOptionsDictionariesSingleton { get; } = new ();

        private static List<Assembly> _assemblyCollection { get; } = new ();

        private static List<Assembly> _registeredAssemblyCollection { get; } = new ();

        private static List<Type> _busTypedExceptionMessageContainingAnHandler { get; } = new ();

        private static List<Type> _busTypedLogMessageContainingAnHandler { get; } = new ();

        public static ICollection<Assembly> GetAssemblies()
            => _assemblyCollection;

        public static ICollection<Assembly> GetRegisteredAssemblies()
            => _registeredAssemblyCollection;

        public static ICollection<Type> GetBusTypedExceptionMessageContainingAnHandlerCollection()
            => _busTypedExceptionMessageContainingAnHandler;

        public static ICollection<Type> GetBusTypedLogMessageContainingAnHandlerCollection()
            => _busTypedLogMessageContainingAnHandler;

        public static BusSerializerManager TryRegisterAndGetBusSerializerManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyBusSerializerManagerSingleton is null)
                    _lazyBusSerializerManagerSingleton = new BusSerializerManager();

                services.TryAddSingleton<IBusSerializerManager>(_lazyBusSerializerManagerSingleton);

                return _lazyBusSerializerManagerSingleton;
            }
        }

        public static EventMiddlewareManager TryRegisterAndGetMessageMiddlewareManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyMessageMiddlewareManagerSingleton is null)
                    _lazyMessageMiddlewareManagerSingleton = new EventMiddlewareManager(services);

                services.TryAddSingleton<IEventMiddlewareManager>(_lazyMessageMiddlewareManagerSingleton);
                services.TryAddSingleton(prov => prov.GetService(typeof(IEventMiddlewareManager)) as IEventMiddlewareExecutorProvider);
                services.TryAddSingleton(prov => prov.GetService(typeof(IEventMiddlewareManager)) as IEventMiddlewareRegister);

                return _lazyMessageMiddlewareManagerSingleton;
            }
        }

        public static MessageTypeCache TryRegisterAndGetMessageTypeCache(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyMessageTypeCacheSingleton is null)
                    _lazyMessageTypeCacheSingleton = new MessageTypeCache();

                services.TryAddSingleton<IMessageTypeCache>(_lazyMessageTypeCacheSingleton);

                return _lazyMessageTypeCacheSingleton;
            }
        }

        public static ProducerManager TryRegisterAndGetProducerManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyProducerManagerSingleton is null)
                    _lazyProducerManagerSingleton = new ProducerManager(services);

                services.TryAddSingleton<IEventDispatcher, MessageProducer>();
                services.TryAddSingleton<IProducerManager>(_lazyProducerManagerSingleton);

                return _lazyProducerManagerSingleton;
            }
        }

        public static BusOptionsDictionary<TOptions> TryRegisterAndGetBusOptionsDictionary<TOptions>(IServiceCollection services)
            where TOptions : class, new()
        {
            lock (_lock)
            {
                if (!_busOptionsDictionariesSingleton.TryGetValue(typeof(TOptions), out var dict))
                {
                    dict = new BusOptionsDictionary<TOptions>();
                    _busOptionsDictionariesSingleton.TryAdd(typeof(TOptions), dict);
                }
                var ret = dict as BusOptionsDictionary<TOptions>;
                services.TryAddSingleton<IBusOptionsDictionary<TOptions>>(ret);

                return ret;
            }
        }
    }
}
