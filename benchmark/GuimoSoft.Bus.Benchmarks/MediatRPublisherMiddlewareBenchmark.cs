using BenchmarkDotNet.Attributes;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Benchmarks.Fakes;
using GuimoSoft.Bus.Benchmarks.Handlers.Benchmark;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Benchmarks
{
    [Description("Consuming and send via MediatorPublisherMiddleware")]
    public class MediatRPublisherMiddlewareBenchmark : BenchmarkBase
    {
        private IConsumer<string, byte[]> Consumer { get; set; }

        [GlobalSetup]
        public override Task GlobalSetupAsync()
        {
            var services = new ServiceCollection();
            services
                .InjectInMemoryKafka()
                .AddMediatR(typeof(BenchmarkBase).Assembly)
                .AddSingleton<MediatorPublisherMiddleware<BenchmarkMessage>>();

            Services = services.BuildServiceProvider(true);

            Producer = Services.GetRequiredService<IEventDispatcher>();
            var consumerBuilder = Services.GetRequiredService<IKafkaConsumerBuilder>();

            Consumer = consumerBuilder.Build(ServerName.Default);

            Consumer.Subscribe(BenchmarkMessage.TOPIC_NAME);

            return Task.CompletedTask;
        }

        [GlobalCleanup]
        public override Task GlobalCleanupAsync()
        {
            Consumer.Unsubscribe();
            Consumer.Close();
            Services.Dispose();
            return Task.CompletedTask;
        }

        [Benchmark(Description = "produce and consume message")]
        public override async Task ProduceAndConsume()
        {
            await Produce();
            var result = Consumer.Consume(CancellationTokenSource.Token);
            var message = JsonMessageSerializer.Instance.Deserialize(typeof(BenchmarkMessage), result.Message.Value) as BenchmarkMessage;
            var messageContext = new ConsumeContext<BenchmarkMessage>(message, Services, new ConsumeInformations(BusName.Kafka, ServerName.Default, BenchmarkMessage.TOPIC_NAME), CancellationTokenSource.Token);
            var mediator = Services.GetRequiredService<MediatorPublisherMiddleware<BenchmarkMessage>>();
            await mediator.InvokeAsync(messageContext, () => Task.CompletedTask);
            WaitId();
        }
    }
}
