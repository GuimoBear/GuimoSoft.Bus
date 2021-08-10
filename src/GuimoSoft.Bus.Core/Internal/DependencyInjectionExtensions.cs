using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Core.Internal
{
    internal static class DependencyInjectionExtensions
    {
        public static IServiceCollection RegisterMediatorFromNewAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            lock (Singletons._lock)
            {
                var registeredAssemblies = Singletons.GetRegisteredAssemblies();
                var unregisteredAssemblies = assemblies.Where(a => !registeredAssemblies.Contains(a)).ToList();
                if (unregisteredAssemblies.Count > 0)
                {
                    services.AddMediatR(unregisteredAssemblies.ToArray());
                    DiscoveryAndRegisterTypedExceptionMessageHandlers(unregisteredAssemblies);
                    DiscoveryAndRegisterTypedLogMessageHandlers(unregisteredAssemblies);
                    unregisteredAssemblies.ForEach(registeredAssemblies.Add);
                }
                return services;
            }
        }

        private static void DiscoveryAndRegisterTypedExceptionMessageHandlers(IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedExceptionMessages = GetTypesFromGenericNotificationHandlers(assemblies, typeof(BusTypedExceptionMessage<>));
            var registeredExceptionHAndlerTypes = Singletons.GetBusTypedExceptionMessageContainingAnHandlerCollection();

            registeredBusTypedExceptionMessages
                .Where(messageType => !registeredExceptionHAndlerTypes.Contains(messageType))
                .ToList()
                .ForEach(messageType =>
                {
                    _ = DelegateCache.GetOrAddBusLogMessageFactory(messageType);
                    registeredExceptionHAndlerTypes.Add(messageType);
                });
        }

        private static void DiscoveryAndRegisterTypedLogMessageHandlers(IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedLogMessages = GetTypesFromGenericNotificationHandlers(assemblies, typeof(BusTypedLogMessage<>));
            var registeredLogHAndlerTypes = Singletons.GetBusTypedLogMessageContainingAnHandlerCollection();

            registeredBusTypedLogMessages
                .Where(messageType => !registeredLogHAndlerTypes.Contains(messageType))
                .ToList()
                .ForEach(messageType =>
                {
                    _ = DelegateCache.GetOrAddBusLogMessageFactory(messageType);
                    registeredLogHAndlerTypes.Add(messageType);
                });
        }

        private static List<Type> GetTypesFromGenericNotificationHandlers(IEnumerable<Assembly> assemblies, Type genericNotificationTypeDefinition)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetInterfaces())
                .Where(intr => intr.IsGenericType &&
                               intr.GetGenericTypeDefinition().Equals(typeof(INotificationHandler<>)))
                .Select(handler => handler.GetGenericArguments()[0])
                .Where(messageType => messageType.IsGenericType &&
                                      messageType.GetGenericTypeDefinition().Equals(genericNotificationTypeDefinition))
                .Select(messageType => messageType.GetGenericArguments()[0])
                .ToList();
        }
    }
}
