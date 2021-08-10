using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaMessageConsumerStarterTests
    {
        [Fact]
        public void StartConsumersShouldStartSingleConsumerPerMessage()
        {
            var mockMessageTypeCache = new Mock<IMessageTypeCache>();
            mockMessageTypeCache
                .Setup(x => x.GetSwitchers(BusName.Kafka, Finality.Consume))
                .Returns(new List<Enum> { ServerName.Default });
            mockMessageTypeCache
                .Setup(x => x.GetEndpoints(BusName.Kafka, Finality.Consume, ServerName.Default))
                .Returns(new List<string> { FakeMessage.TOPIC_NAME, OtherFakeMessage.TOPIC_NAME, AnotherFakeMessage.TOPIC_NAME });

            var mockKafkaMessageConsumer = new Mock<IKafkaTopicMessageConsumer>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockKafkaMessageConsumer.Object);
            serviceCollection.AddSingleton(s => mockMessageTypeCache.Object);
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<FakeMessage>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<OtherFakeMessage>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<AnotherFakeMessage>>());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sut = new KafkaMessageConsumerManager(serviceProvider);
            sut.StartConsumers(CancellationToken.None);

            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), FakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), OtherFakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
            mockKafkaMessageConsumer.Verify(x => x.ConsumeUntilCancellationIsRequested(It.IsAny<Enum>(), AnotherFakeMessage.TOPIC_NAME, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}