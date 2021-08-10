using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class MessageProducerTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageProducer(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new MessageProducer(Mock.Of<IProducerManager>(), null, null));
            Assert.Throws<ArgumentNullException>(() => new MessageProducer(Mock.Of<IProducerManager>(), Mock.Of<IMessageTypeCache>(), null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfKeyIsNullEmptyOrWriteSpace(string key)
        {
            var sut = new MessageProducer(Mock.Of<IProducerManager>(), Mock.Of<IMessageTypeCache>(), Mock.Of<IServiceProvider>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.DispatchAsync(key, new FakeMessage("", ""), CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldBeThrowArgumentNullExceptionIfMessageIsNull()
        {
            var sut = new MessageProducer(Mock.Of<IProducerManager>(), Mock.Of<IMessageTypeCache>(), Mock.Of<IServiceProvider>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DispatchAsync<FakeMessage>(Guid.NewGuid().ToString(), null, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldGetProduceInformationsGetProducesAndProduce()
        {
            (BusName Bus, Enum Switch, string Endpoint) expectedPublishParameters = (BusName.Kafka, ServerName.Default, FakeMessage.TOPIC_NAME);
            var expectedKey = Guid.NewGuid().ToString();
            var expectedMessage = new FakeMessage("test", "test");

            var moqBusMessageProducer = new Mock<IBusEventDispatcher>();

            var moqProducerManager = new Mock<IProducerManager>();
            moqProducerManager
                .Setup(x => x.GetProducer(BusName.Kafka, It.IsAny<IServiceProvider>()))
                .Returns(() => moqBusMessageProducer.Object);

            var moqMessageTypeCache = new Mock<IMessageTypeCache>();
            moqMessageTypeCache
                .Setup(x => x.Get(It.IsAny<Type>()))
                .Returns(new List<(BusName BusName, Enum Switch, string Endpoint)> { expectedPublishParameters });

            var sut = new MessageProducer(moqProducerManager.Object, moqMessageTypeCache.Object, Mock.Of<IServiceProvider>());

            await sut.DispatchAsync(expectedKey, expectedMessage, CancellationToken.None);

            moqMessageTypeCache
                .Verify(x => x.Get(typeof(FakeMessage)), Times.Once);

            moqProducerManager
                .Verify(x => x.GetProducer(expectedPublishParameters.Bus, It.IsAny<IServiceProvider>()), Times.Once());

            moqBusMessageProducer
                .Verify(x => x.DispatchAsync(expectedKey, expectedMessage, expectedPublishParameters.Switch, expectedPublishParameters.Endpoint, CancellationToken.None), Times.Once());
        }
    }
}
