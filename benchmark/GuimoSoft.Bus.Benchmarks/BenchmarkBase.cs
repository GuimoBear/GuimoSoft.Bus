using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Benchmarks.Handlers.Benchmark;

namespace GuimoSoft.Bus.Benchmarks
{
    public abstract class BenchmarkBase
    {
        protected ServiceProvider Services { get; set; }
        protected IEventDispatcher Producer { get; set; }

        private Guid _currentId;

        protected readonly CancellationTokenSource CancellationTokenSource = new();

        public abstract Task GlobalSetupAsync();

        public abstract Task GlobalCleanupAsync();

        public abstract Task ProduceAndConsume();

        protected async Task Produce()
            => await Producer.DispatchAsync(BenchmarkMessage.TOPIC_NAME, new BenchmarkMessage(_currentId = Guid.NewGuid()));

        protected void WaitId()
        {
            while (!(BenchmarkContext.TryGet(out var id) && _currentId == id)) ;
        }
    }
}
