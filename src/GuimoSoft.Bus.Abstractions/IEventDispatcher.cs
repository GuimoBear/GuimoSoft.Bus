using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions
{
    public interface IEventDispatcher
    {
        Task DispatchAsync<TEvent>(string key, TEvent message, CancellationToken cancellationToken = default) 
            where TEvent : IEvent;
    }
}