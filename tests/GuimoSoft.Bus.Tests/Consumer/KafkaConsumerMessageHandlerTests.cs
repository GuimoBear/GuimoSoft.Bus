using Moq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Kafka.Consumer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerMessageHandlerTests
    {
        [Fact]
        public async Task KafkaConsumerMessageHandlerFacts()
        {
            var moqKafkaMessageConsumerManager = new Mock<IKafkaMessageConsumerManager>();

            var sut = new KafkaConsumerMessageHandler(moqKafkaMessageConsumerManager.Object);

            using var cts = new CancellationTokenSource();

            await sut.StartAsync(cts.Token);
            await Task.Delay(500);
            await sut.StopAsync(cts.Token);

            moqKafkaMessageConsumerManager
                .Verify(x => x.StartConsumers(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
