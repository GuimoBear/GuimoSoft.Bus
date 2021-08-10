using GuimoSoft.Bus.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Examples.Handlers.HelloMessage
{
    public class HelloMessageHandler : INotificationHandler<Messages.HelloMessage>
    {
        private readonly ILogger<HelloMessageHandler> _logger;
        private readonly IConsumeContextAccessor<Messages.HelloMessage> _contextAccessor;

        public HelloMessageHandler(ILogger<HelloMessageHandler> logger, IConsumeContextAccessor<Messages.HelloMessage> contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Handle(Messages.HelloMessage notification, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogInformation($"Hello {notification.Name}!");
        }
    }
}
