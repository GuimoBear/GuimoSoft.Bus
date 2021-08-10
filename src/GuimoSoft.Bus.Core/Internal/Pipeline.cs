using Sigil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class Pipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        private readonly Func<ConsumeContextBase, Task> _executor = null;

        public Pipeline(IReadOnlyList<Type> middlewareTypes, Type messageType)
        {
            if (middlewareTypes is null)
                throw new ArgumentNullException(nameof(middlewareTypes));
            if (!typeof(IEvent).IsAssignableFrom(messageType))
                throw new ArgumentException($"{messageType.Name} deve implementar a interface {nameof(IEvent)}");

            var middlewareType = typeof(IEventMiddleware<>).MakeGenericType(messageType);
            if (middlewareTypes.Any(mt => !middlewareType.IsAssignableFrom(mt)))
                throw new ArgumentException($"Todos os middlewares devem implementar a interface {middlewareType.Name}<{messageType.Name}>");

            _middlewareTypes = middlewareTypes.Reverse().ToList();

            _executor = GetExecutor(typeof(ConsumeContext<>).MakeGenericType(messageType));
        }

        public async Task<ConsumeContextBase> Execute(object message, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            var ctx = CreateContext(message.GetType(), message, services, informations, cancellationToken);
            await _executor(ctx);
            return ctx;
        }

        private Func<ConsumeContextBase, Task> GetExecutor(Type contextType)
        {
            Func<ConsumeContextBase, Task> source = _ => Task.CompletedTask;
            foreach (var middlewareExecutor in CreateMiddlewareExecutors(contextType))
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            return source;
        }

        private static Func<ConsumeContextBase, Task> CreatePipelineLevelExecutor(Func<ConsumeContextBase, Task> source, Func<ConsumeContextBase, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, ConsumeInformations, CancellationToken, ConsumeContextBase>> _constructorDelegates
            = new ();

        private static ConsumeContextBase CreateContext(Type messageType, object message, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            if (!_constructorDelegates.TryGetValue(messageType, out var ctor))
            {
                var constructorInfo = typeof(ConsumeContext<>).MakeGenericType(messageType)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
                        Type.DefaultBinder,
                        new Type[] { messageType, typeof(IServiceProvider), typeof(ConsumeInformations), typeof(CancellationToken) },
                        Array.Empty<ParameterModifier>());

                ctor = Emit<Func<object, IServiceProvider, ConsumeInformations, CancellationToken, ConsumeContextBase>>.NewDynamicMethod($"{messageType.Name}_MessageContext_Ctor")
                    .LoadArgument(0)
                    .CastClass(messageType)
                    .LoadArgument(1)
                    .LoadArgument(2)
                    .LoadArgument(3)
                    .NewObject(constructorInfo)
                    .Return()
                    .CreateDelegate();

                _constructorDelegates.TryAdd(messageType, ctor);
            }
            return ctor(message, services, informations, cancellationToken);
        }

        private static readonly ConcurrentDictionary<Type, Func<object, ConsumeContextBase, Func<Task>, Task>> _middlewareExecutorsCache
            = new();

        private IEnumerable<Func<ConsumeContextBase, Func<Task>, Task>> CreateMiddlewareExecutors(Type messageContextType)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareInvokeDelegate))
                {
                    var middlewareExecutor = middlewareType.GetMethod("InvokeAsync", new Type[] { messageContextType, typeof(Func<Task>) });

                    var emmiter = Emit<Func<object, ConsumeContextBase, Func<Task>, Task>>
                        .NewDynamicMethod($"{middlewareType.Name}_InvokeAsync")
                        .LoadArgument(0)
                        .CastClass(middlewareType)
                        .LoadArgument(1)
                        .CastClass(messageContextType)
                        .LoadArgument(2)
                        .Call(middlewareExecutor)
                        .Return();

                    middlewareInvokeDelegate = emmiter.CreateDelegate();

                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareInvokeDelegate);
                }
                yield return async (obj, next) => await middlewareInvokeDelegate(obj.Services.GetService(middlewareType), obj, next);
            }
        }
    }
}
