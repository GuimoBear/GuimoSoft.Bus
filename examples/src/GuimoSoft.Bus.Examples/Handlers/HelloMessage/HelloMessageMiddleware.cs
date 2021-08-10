using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Examples.Handlers.HelloMessage
{
    public class HelloMessageMiddleware : IEventMiddleware<Messages.HelloMessage>
    {
        private readonly ILogger<HelloMessageMiddleware> _logger;

        public HelloMessageMiddleware(ILogger<HelloMessageMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(ConsumeContext<Messages.HelloMessage> context, Func<Task> next)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger.LogInformation(context.Message.Name);

            if (context.Message.ThrowException)
                throw new ArgumentException(nameof(context));

            await next();
        }
    }
}
