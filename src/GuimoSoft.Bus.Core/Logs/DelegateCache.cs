using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace GuimoSoft.Bus.Core.Logs
{
    internal static class DelegateCache
    {
        private static readonly ConcurrentDictionary<Type, TypedLogMessageFactory> _typedBusLogMessageFactories = new();
        private static readonly ConcurrentDictionary<Type, TypedExceptionMessageFactory> _typedBusExceptionMessageFactories = new();

        public static TypedLogMessageFactory GetOrAddBusLogMessageFactory(Type messageType)
            => _typedBusLogMessageFactories.GetOrAdd(messageType, CreateBusLogMessageFactory);

        public static TypedExceptionMessageFactory GetOrAddBusExceptionMessageFactory(Type messageType)
            => _typedBusExceptionMessageFactories.GetOrAdd(messageType, CreateBusExceptionMessageFactory);

        private static TypedLogMessageFactory CreateBusLogMessageFactory(Type messageType)
        {
            var logMessageConstructor = typeof(BusTypedLogMessage<>).MakeGenericType(messageType)
                .GetConstructor(new Type[] { typeof(BusLogMessage), messageType });

            return Emit<TypedLogMessageFactory>
                .NewDynamicMethod($"{messageType.Name}LogMessage_Ctor")
                .LoadArgument(0)
                .LoadArgument(1)
                .CastClass(messageType)
                .NewObject(logMessageConstructor)
                .Return()
                .CreateDelegate();
        }

        private static TypedExceptionMessageFactory CreateBusExceptionMessageFactory(Type messageType)
        {
            var logMessageConstructor = typeof(BusTypedExceptionMessage<>).MakeGenericType(messageType)
                .GetConstructor(new Type[] { typeof(BusExceptionMessage), messageType });

            return Emit<TypedExceptionMessageFactory>
                .NewDynamicMethod($"{messageType.Name}ExceptionMessage_Ctor")
                .LoadArgument(0)
                .LoadArgument(1)
                .CastClass(messageType)
                .NewObject(logMessageConstructor)
                .Return()
                .CreateDelegate();
        }
    }

    internal delegate object TypedLogMessageFactory(BusLogMessage logMessage, object messageObject);
    internal delegate object TypedExceptionMessageFactory(BusExceptionMessage exceptionMessage, object messageObject);
}
