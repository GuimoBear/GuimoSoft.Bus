using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IEventMiddlewareExecutorProvider
    {
        Pipeline GetPipeline(BusName brokerName, Enum @switch, Type messageType);
    }

    public interface IEventMiddlewareRegister
    {
        void Register<TEvent, TMiddleware>(BusName brokerName, Enum @switch, ServiceLifetime lifetime)
            where TEvent : IEvent
            where TMiddleware : class, IEventMiddleware<TEvent>;

        void Register<TEvent, TMiddleware>(BusName brokerName, Enum @switch, Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime)
            where TEvent : IEvent
            where TMiddleware : class, IEventMiddleware<TEvent>;
    }

    internal interface IEventMiddlewareManager : IEventMiddlewareExecutorProvider, IEventMiddlewareRegister
    {
    }
}
