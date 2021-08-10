using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Abstractions.Consumer
{
    public interface IEventMiddleware<TEvent>
        where TEvent : IEvent
    {
        Task InvokeAsync(ConsumeContext<TEvent> context, Func<Task> next);
    }
}
