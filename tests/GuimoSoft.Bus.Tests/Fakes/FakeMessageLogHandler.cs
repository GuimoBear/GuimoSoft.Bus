using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageLogHandler : INotificationHandler<BusTypedLogMessage<FakeMessage>>
    {
        public Task Handle(BusTypedLogMessage<FakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
