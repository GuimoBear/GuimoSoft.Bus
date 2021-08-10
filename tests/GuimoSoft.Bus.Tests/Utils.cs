using Sigil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Tests
{
    internal static class Utils
    {
        internal static readonly object Lock = new();

        internal static void ResetarMessageSerializerManager()
        {
            typeof(MessageSerializerManager)
                .GetField("_defaultSerializer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(MessageSerializerManager.Instance, JsonMessageSerializer.Instance);

            var dict = typeof(MessageSerializerManager)
                .GetField("_typedSerializers", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(MessageSerializerManager.Instance);

            var method = typeof(ConcurrentDictionary<Type, IDefaultSerializer>)
                .GetMethod(nameof(IDictionary<Type, IDefaultSerializer>.Clear), BindingFlags.Instance | BindingFlags.Public);

            method.Invoke(dict, Array.Empty<object>());
        }

        internal static void ResetarSingletons()
        {
            lock (Singletons._lock)
            {
                lazyBusSerializerManagerSetter.Value(null);
                lazyMessageMiddlewareManagerSetter.Value(null);
                lazyMessageTypeCacheSetter.Value(null);
                lazyProducerManagerSetter.Value(null);
                lazyBusOptionsDictionariesCleaner.Value();
                lazyAssembliesCleaner.Value();
                lazyRegisteredAssembliesCleaner.Value();
                lazyTypedExceptionMessagesCleaner.Value();
                lazyTypedLogMessagesCleaner.Value();
            }
        }

        private static readonly Lazy<Action<BusSerializerManager>> lazyBusSerializerManagerSetter
            = new(CreateBusSerializerManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<EventMiddlewareManager>> lazyMessageMiddlewareManagerSetter
            = new(CreateMessageMiddlewareManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<MessageTypeCache>> lazyMessageTypeCacheSetter
            = new(CreateMessageTypeCacheSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<ProducerManager>> lazyProducerManagerSetter
            = new(CreateProducerManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyBusOptionsDictionariesCleaner
            = new(CreateBusOptionsDictionariesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyAssembliesCleaner
            = new(CreateAssembliesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyRegisteredAssembliesCleaner
            = new(CreateRegisteredAssembliesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyTypedExceptionMessagesCleaner
            = new(CreateTypedExceptionMessagesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyTypedLogMessagesCleaner
            = new(CreateTypedLogMessagesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static Action<BusSerializerManager> CreateBusSerializerManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyBusSerializerManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<BusSerializerManager>>
                .NewDynamicMethod("BusSerializerManagerSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<EventMiddlewareManager> CreateMessageMiddlewareManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyMessageMiddlewareManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<EventMiddlewareManager>>
                .NewDynamicMethod("MessageMiddlewareManagerSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<MessageTypeCache> CreateMessageTypeCacheSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyMessageTypeCacheSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<MessageTypeCache>>
                .NewDynamicMethod("MessageTypeCacheSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<ProducerManager> CreateProducerManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyProducerManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<ProducerManager>>
                .NewDynamicMethod("ProducerManager_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action CreateBusOptionsDictionariesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busOptionsDictionariesSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(ConcurrentDictionary<Type, object>)
                .GetMethod(nameof(ConcurrentDictionary<Type, object>.Clear));

            return Emit<Action>
                .NewDynamicMethod("BusOptionsDictionaries_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateAssembliesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_assemblyCollection", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Assembly>)
                .GetMethod(nameof(List<Assembly>.Clear));

            return Emit<Action>
                .NewDynamicMethod("Assemblies_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateRegisteredAssembliesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_registeredAssemblyCollection", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Assembly>)
                .GetMethod(nameof(List<Assembly>.Clear));

            return Emit<Action>
                .NewDynamicMethod("RegisteredAssemblies_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateTypedExceptionMessagesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busTypedExceptionMessageContainingAnHandler", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Type>)
                .GetMethod(nameof(List<Type>.Clear));

            return Emit<Action>
                .NewDynamicMethod("TypedExceptionMessages_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateTypedLogMessagesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busTypedLogMessageContainingAnHandler", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Type>)
                .GetMethod(nameof(List<Type>.Clear));

            return Emit<Action>
                .NewDynamicMethod("TypedLogMessages_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }
    }
}
