using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Kafka.Producer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class ProducerManagerTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ProducerManager(null));
        }

        [Fact]
        public void AddFacts()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IKafkaProducerBuilder>());
            services.AddSingleton(Mock.Of<IBusSerializerManager>());

            var sut = new ProducerManager(services);

            sut.Add<KafkaMessageProducer>(BusName.Kafka);

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(KafkaMessageProducer))
                .Should().NotBeNull();

            sut.Add<FakeMessageProducer>(BusName.Kafka);

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(KafkaMessageProducer))
                .Should().NotBeNull();

            services
                .FirstOrDefault(sd => sd.ServiceType == typeof(FakeMessageProducer))
                .Should().BeNull();
        }

        [Fact]
        public void GetProducerFacts()
        {
            var services = new ServiceCollection();

            var sut = new ProducerManager(services);

            Assert.Throws<InvalidOperationException>(() => sut.GetProducer(BusName.Kafka, Mock.Of<IServiceProvider>()));

            sut.Add<FakeMessageProducer>(BusName.Kafka);

            var moqServiceProvider = new Mock<IServiceProvider>();
            moqServiceProvider
                .Setup(x => x.GetService(typeof(FakeMessageProducer)))
                .Returns(FakeMessageProducer.Instance);

            var producer = sut.GetProducer(BusName.Kafka, moqServiceProvider.Object);

            producer
                .Should().BeSameAs(FakeMessageProducer.Instance);
        }

        private class FakeMessageProducer : IBusEventDispatcher
        {
            internal static readonly FakeMessageProducer Instance = new();

            public Task DispatchAsync<TEvent>(string key, TEvent message, Enum @switch, string endpoint, CancellationToken cancellationToken = default) where TEvent : IEvent
            {
                throw new NotImplementedException();
            }
        }
    }
}
