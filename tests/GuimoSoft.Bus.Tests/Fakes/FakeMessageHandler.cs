using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageHandler : INotificationHandler<FakeMessage>
    {
        public Task Handle(FakeMessage notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
