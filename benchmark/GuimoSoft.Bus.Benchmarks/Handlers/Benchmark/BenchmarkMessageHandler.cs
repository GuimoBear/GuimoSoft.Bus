using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Benchmarks.Handlers.Benchmark
{
    public class BenchmarkMessageHandler : INotificationHandler<BenchmarkMessage>
    {
        public Task Handle(BenchmarkMessage notification, CancellationToken cancellationToken)
        {
            BenchmarkContext.Add(notification.Id);
            return Task.CompletedTask;
        }
    }
}
