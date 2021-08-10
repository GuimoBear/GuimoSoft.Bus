using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Core.Internal
{
    internal sealed class ConsumeContextAccessorInitializerMiddleware<TEvent> : IEventMiddleware<TEvent>
        where TEvent : IEvent
    {
        private readonly IConsumeContextAccessor<TEvent> contextAccessor;

        public ConsumeContextAccessorInitializerMiddleware(IConsumeContextAccessor<TEvent> contextAccessor)
        {
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task InvokeAsync(ConsumeContext<TEvent> context, Func<Task> next)
        {
            try
            {
                contextAccessor.Context = context;
                await next();
            }
            finally
            {
                contextAccessor.Context = null;
            }
        }
    }
}
