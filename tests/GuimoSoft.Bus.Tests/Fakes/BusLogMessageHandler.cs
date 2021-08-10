using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusLogMessageHandler : INotificationHandler<BusLogMessage>
    {
        public Task Handle(BusLogMessage notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
