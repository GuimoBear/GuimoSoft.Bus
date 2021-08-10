using MediatR;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Core.Logs;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageExceptionHandler : INotificationHandler<BusTypedExceptionMessage<FakeMessage>>
    {
        public Task Handle(BusTypedExceptionMessage<FakeMessage> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
