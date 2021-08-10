using Confluent.Kafka;
using Moq;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Producer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaProducerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleProducerBuilder()
        {
            var sut = new KafkaProducerBuilder(Mock.Of<IBusOptionsDictionary<ProducerConfig>>(), Mock.Of<IBusLogDispatcher>());
            Assert.IsType<KafkaProducerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(null, Mock.Of<IBusLogDispatcher>()));
            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(Mock.Of<IBusOptionsDictionary<ProducerConfig>>(), null));
        }

        [Fact]
        public void BuildShouldReturnNonNullProducer()
        {
            var moqOptionsDictionary = new Mock<IBusOptionsDictionary<ProducerConfig>>();

            var sut = new KafkaProducerBuilder(Mock.Of<IBusOptionsDictionary<ProducerConfig>>(), Mock.Of<IBusLogDispatcher>());
            
            var kafkaOptions = new ProducerConfig();

            moqOptionsDictionary
                .Setup(x => x.TryGetValue(ServerName.Default, out kafkaOptions))
                .Returns(false);

            sut = new KafkaProducerBuilder(moqOptionsDictionary.Object, Mock.Of<IBusLogDispatcher>());

            Assert.Throws<KeyNotFoundException>(() => sut.Build(ServerName.Default));

            moqOptionsDictionary
                .Setup(x => x.TryGetValue(ServerName.Default, out kafkaOptions))
                .Returns(true);

            var producer = sut.Build(ServerName.Default);

            Assert.NotNull(producer);
        }
    }
}