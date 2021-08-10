using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Core.Internal
{
    internal sealed class MediatorPublisherMiddleware<TEvent> : IEventMiddleware<TEvent>
        where TEvent : IEvent
    {
        public async Task InvokeAsync(ConsumeContext<TEvent> context, Func<Task> next)
        {
            var mediator = context.Services.GetRequiredService<IMediator>();
            await mediator.Publish(context.Message, context.CancellationToken).ConfigureAwait(false);
            await next();
        }
    }
}
