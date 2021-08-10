using Confluent.Kafka;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Consumer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleConsumerBuilder()
        {
            var sut = new KafkaConsumerBuilder(Mock.Of<IBusOptionsDictionary<ConsumerConfig>>(), Mock.Of<IBusLogDispatcher>());
            Assert.IsType<KafkaConsumerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(null, Mock.Of<IBusLogDispatcher>()));
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerBuilder(Mock.Of<IBusOptionsDictionary<ConsumerConfig>>(), null));
        }

        [Fact]
        public void BuildShouldReturnNonNullConsumer()
        {
            var moqDictionary = new Mock<IBusOptionsDictionary<ConsumerConfig>>();

            var sut = new KafkaConsumerBuilder(moqDictionary.Object, Mock.Of<IBusLogDispatcher>());

            Assert.Throws<KeyNotFoundException>(() => sut.Build(ServerName.Default));

            var kafkaOptions = new ConsumerConfig() { GroupId = "fake-group-id" };
            moqDictionary
                .Setup(x => x.TryGetValue(ServerName.Default, out kafkaOptions))
                .Returns(true);

            var producer = sut.Build(ServerName.Default);

            Assert.NotNull(producer);
        }
    }
}