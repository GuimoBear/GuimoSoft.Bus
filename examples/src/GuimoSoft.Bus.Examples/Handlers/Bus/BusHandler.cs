using GuimoSoft.Bus.Core.Logs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Examples.Handlers.Bus
{
    public class BusHandler : INotificationHandler<BusLogMessage>, INotificationHandler<BusExceptionMessage>
    {
        private readonly ILogger<BusHandler> _logger;

        public BusHandler(ILogger<BusHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(BusLogMessage notification, CancellationToken cancellationToken)
        {
            _logger.Log((LogLevel)(notification.Level), notification.Message);
            return Task.CompletedTask;
        }

        public Task Handle(BusExceptionMessage notification, CancellationToken cancellationToken)
        {
            _logger.Log((LogLevel)(notification.Level), notification.Message);
            return Task.CompletedTask;
        }
    }
}
