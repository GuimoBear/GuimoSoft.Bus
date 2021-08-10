using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeMessageHandler : INotificationHandler<SecondFakeMessage>
    {
        public Task Handle(SecondFakeMessage notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
