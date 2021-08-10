using MediatR;
using Moq;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class ClientBuilderTests
    {
        private static (Mock<IBusLogDispatcher>, Mock<IMediator>) CreateLoggerMock(BusName bus)
        {
            var moqMediator = new Mock<IMediator>();

            var moqLogger = new Mock<IBusLogDispatcher>();

            moqLogger
                .Setup(x => x.FromBus(bus)).Returns(() => new BusLogDispatcherBuilder(moqMediator.Object, bus));

            return (moqLogger, moqMediator);
        }

        [Fact]
        public void LogMessageFacts()
        {
            var (moqLogger, moqMediator) = CreateLoggerMock(BusName.Kafka);

            var sut = new FakeClientBuilder(moqLogger.Object);

            sut.WriteLog();

            moqMediator
                .Verify(x => x.Publish(It.IsAny<BusLogMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void LogExceptionFacts()
        {
            var (moqLogger, moqMediator) = CreateLoggerMock(BusName.Kafka);

            var sut = new FakeClientBuilder(moqLogger.Object);

            sut.WriteException();

            moqMediator
                .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
