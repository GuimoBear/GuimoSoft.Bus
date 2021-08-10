using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class BusExceptionMessageHandler : INotificationHandler<BusExceptionMessage>
    {
        public Task Handle(BusExceptionMessage notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
