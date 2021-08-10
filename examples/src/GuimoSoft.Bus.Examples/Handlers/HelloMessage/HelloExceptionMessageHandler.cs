using GuimoSoft.Bus.Core.Logs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Examples.Handlers.HelloMessage
{
    public class HelloExceptionMessageHandler : INotificationHandler<BusTypedExceptionMessage<Messages.HelloMessage>>
    {
        private readonly ILogger<HelloExceptionMessageHandler> _logger;

        public HelloExceptionMessageHandler(ILogger<HelloExceptionMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusTypedExceptionMessage<Messages.HelloMessage> notification, CancellationToken cancellationToken)
        {
            _logger.Log((LogLevel)(notification.Level), notification.Message);
            return Task.CompletedTask;
        }
    }
}
