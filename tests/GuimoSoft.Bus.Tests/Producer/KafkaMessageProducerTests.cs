using Confluent.Kafka;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaMessageProducerTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaMessageProducer(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaMessageProducer(Mock.Of<IKafkaProducerBuilder>(), null));
            Assert.Throws<ArgumentNullException>(() => new KafkaMessageProducer(null, Mock.Of<IBusSerializerManager>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfKeyIsNullEmptyOrWriteSpace(string key)
        {
            var sut = new KafkaMessageProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.DispatchAsync(key, new FakeMessage("", ""), ServerName.Default, FakeMessage.TOPIC_NAME, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldBeThrowArgumentNullExceptionIfMessageIsNull()
        {
            var sut = new KafkaMessageProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DispatchAsync<FakeMessage>(Guid.NewGuid().ToString(), null, ServerName.Default, FakeMessage.TOPIC_NAME, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfEndpointIsNullEmptyOrWriteSpace(string endpoint)
        {
            var sut = new KafkaMessageProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.DispatchAsync(Guid.NewGuid().ToString(), new FakeMessage("", ""), ServerName.Default, endpoint, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldSerializeAndSendMessageToKafka()
        {
            var moqMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            moqMessageProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(moqMessageProducerBuilder.Object, moqSerializerManager.Object);

            await sut.DispatchAsync(Guid.NewGuid().ToString(), new FakeMessage("", ""), ServerName.Default, FakeMessage.TOPIC_NAME);

            moqSerializerManager
                .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage)), Times.Once);

            moqMessageProducerBuilder
                .Verify(x => x.Build(ServerName.Default), Times.Once);

            mockProducer
                .Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DisposeShouldDisposeProducerIfProduceHasBeenCalled()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.DispatchAsync(Guid.NewGuid().ToString(), new FakeMessage("", ""), ServerName.Default, FakeMessage.TOPIC_NAME);
            sut.Dispose();

            mockProducer.Verify(x => x.Dispose());
        }

        [Fact]
        public void DisposeShouldNotDisposeProducerIfProduceHasNotBeenCalled()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            sut.Dispose();

            moqSerializerManager
                .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage)), Times.Never);

            mockProducer.Verify(x => x.Dispose(), Times.Never);
        }
    }
}