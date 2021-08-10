using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Benchmarks.Fakes;
using GuimoSoft.Bus.Benchmarks.Handlers.Benchmark;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Consumer;

namespace GuimoSoft.Bus.Benchmarks
{
    [Description("Consuming using all Bus features")]
    public class BusBenchmark : BenchmarkBase
    {
        private IKafkaMessageConsumerManager ConsumerManager { get; set; }
        private Task consumerTask;

        [GlobalSetup]
        public override async Task GlobalSetupAsync()
        {
            var services = new ServiceCollection();
            services
                .AddKafkaConsumer(configs =>
                    configs
                        .Consume()
                            .OfType<BenchmarkMessage>()
                            .FromEndpoint(BenchmarkMessage.TOPIC_NAME)
                        .FromServer(options => { }))
                .InjectInMemoryKafka();

            Services = services.BuildServiceProvider(true);

            Producer = Services.GetRequiredService<IEventDispatcher>();
            ConsumerManager = Services.GetRequiredService<IKafkaMessageConsumerManager>();
            consumerTask = Task.Factory.StartNew(() => ConsumerManager.StartConsumers(CancellationTokenSource.Token));
            await Task.Delay(10);
        }

        [GlobalCleanup]
        public override async Task GlobalCleanupAsync()
        {
            try
            {
                CancellationTokenSource.Cancel();
                await consumerTask;
                Services.Dispose();
            }
            catch {}
        }

        [Benchmark(Description = "produce and consume message")]
        public override async Task ProduceAndConsume()
        {
            await Produce();
            WaitId();
        }
    }
}
