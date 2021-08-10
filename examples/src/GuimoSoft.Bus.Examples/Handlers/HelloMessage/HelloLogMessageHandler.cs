using GuimoSoft.Bus.Core.Logs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Examples.Handlers.HelloMessage
{
    public class HelloLogMessageHandler : INotificationHandler<BusTypedLogMessage<Messages.HelloMessage>>
    {
        private readonly ILogger<HelloLogMessageHandler> _logger;

        public HelloLogMessageHandler(ILogger<HelloLogMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusTypedLogMessage<Messages.HelloMessage> notification, CancellationToken cancellationToken)
        {
            _logger.Log((LogLevel)(notification.Level), notification.Message);
            return Task.CompletedTask;
        }
    }
}
