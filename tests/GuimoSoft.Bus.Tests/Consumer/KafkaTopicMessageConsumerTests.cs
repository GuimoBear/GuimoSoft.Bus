using Confluent.Kafka;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaTopicMessageConsumerTests
    {
        private static (Mock<IBusLogDispatcher>, Mock<IMediator>) CreateLoggerMock<TEvent>(BusName bus)
            where TEvent : IEvent
        {
            var moqMediator = new Mock<IMediator>();

            var logBuilder = new BusLogDispatcherBuilder(moqMediator.Object, bus);

            var moqLogger = new Mock<IBusLogDispatcher>();

            moqLogger
                .Setup(x => x.FromBus(bus)).Returns(logBuilder);

            return (moqLogger, moqMediator);
        }

        private static Mock<IMessageTypeCache> CreateMessageTypeTopicCache(string topicName)
        {
            var mockMessageTypeCache = new Mock<IMessageTypeCache>();

            mockMessageTypeCache
                .Setup(x => x.Get(BusName.Kafka, Finality.Consume, ServerName.Default, topicName))
                .Returns(new List<Type> { typeof(FakeMessage) });

            return mockMessageTypeCache;
        }

        [Fact]
        public void ConstructorShouldThrowArgumentNullExceptionIfBusLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(null, null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(Mock.Of<IKafkaConsumerBuilder>(), null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IMessageTypeCache>(), null, null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IMessageTypeCache>(), Mock.Of<IEventMiddlewareExecutorProvider>(), null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaTopicMessageConsumer(Mock.Of<IKafkaConsumerBuilder>(), Mock.Of<IServiceProvider>(), Mock.Of<IMessageTypeCache>(), Mock.Of<IEventMiddlewareExecutorProvider>(), Mock.Of<IBusSerializerManager>(), null));
        }

        [Fact]
        public void StartConsumingSubscribesToCorrectTopic()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                const string expectedTopic = "fake-messages";

                var stubCache = CreateMessageTypeTopicCache(expectedTopic);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                // throw exception to avoid infinite loop
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, expectedTopic, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Subscribe(expectedTopic));

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionMessage<FakeMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingConsumesMessageFromConsumer()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);
                // throw exception to avoid infinite loop
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Consume(It.IsAny<CancellationToken>()));

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionMessage<FakeMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingClosesConsumerWhenCancelled()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubMediator = Mock.Of<IMediator>();
                var serviceProvider = BuildServiceProvider(stubMediator);
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                var mockConsumer = new Mock<IConsumer<string, byte[]>>();
                mockConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Throws<OperationCanceledException>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(mockConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cts.Token));
                Thread.Sleep(50);
                cts.Cancel();
                task.Wait();

                mockConsumer.Verify(x => x.Close());

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionMessage<FakeMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingWithSerializerThrowingAnExceptionLogThisException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    }); ;
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqDefaultSerializer = new Mock<IDefaultSerializer>();
                moqDefaultSerializer
                    .Setup(x => x.Deserialize(It.IsAny<Type>(), It.IsAny<byte[]>()))
                    .Throws<Exception>();

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(moqDefaultSerializer.Object);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusTypedExceptionMessage<FakeMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingWithSerializerReturningNullLogMessage()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    }); ;
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(Mock.Of<IDefaultSerializer>());

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusLogMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    }); ;
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
                task.Wait();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddleware()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
                {
                    middlewareExecuted = true;
                });

                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    });
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddlewareThrowingAnException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
                {
                    middlewareExecuted = true;
                    throw new Exception();
                });

                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage, true);
                    }); ;
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                mockMediator.Verify(x =>
                    x.Publish(
                        It.Is<object>(i => i.GetType() == typeof(FakeMessage)),
                        It.IsAny<CancellationToken>()), Times.Never);

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesConsumedMessageToMediatorWithMiddlewareThrowingOperationCanceledException()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var middlewareExecuted = false;

                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();

                var serviceProvider = BuildServiceProviderWithMiddleware<FakeMessage>(mockMediator.Object, message =>
                {
                    middlewareExecuted = true;
                    throw new OperationCanceledException();
                });

                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    }); ;
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);
                var task = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

                task.Wait();

                mockMediator.Verify(x =>
                    x.Publish(
                        It.Is<object>(i => i.GetType() == typeof(FakeMessage)),
                        It.IsAny<CancellationToken>()), Times.Never);

                middlewareExecuted.Should().BeTrue();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);
            }
        }

        [Fact]
        public void StartConsumingPublishesWithTopicCacheThrowsExceptionConsumedMessageToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME + "s");
                stubCache
                    .Setup(cache => cache.Get(BusName.Kafka, Finality.Consume, ServerName.Default, FakeMessage.TOPIC_NAME))
                    .Throws<KeyNotFoundException>();

                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeResult(fakeMessage);
                    });
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                Assert.Throws<KeyNotFoundException>(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));

                stubCache
                    .Verify(x => x.Get(BusName.Kafka, Finality.Consume, ServerName.Default, FakeMessage.TOPIC_NAME), Times.Once);

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Never);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void StartConsumingPublishesInvalidMessageConsumedMessageToMediator()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var fakeMessage = new FakeMessage("some-key-id", "some-property-value");
                var cancellationTokenSource = new CancellationTokenSource();
                var mockMediator = new Mock<IMediator>();
                var serviceProvider = BuildServiceProvider(mockMediator.Object);
                var stubCache = CreateMessageTypeTopicCache(FakeMessage.TOPIC_NAME);
                var stubConsumer = new Mock<IConsumer<string, byte[]>>();
                stubConsumer
                    .Setup(x => x.Consume(It.IsAny<CancellationToken>()))
                    .Returns(() =>
                    {
                        cancellationTokenSource.Cancel();
                        return BuildFakeConsumeWithInvalidMessageResult();
                    });
                var stubMessageConsumerBuilder = new Mock<IKafkaConsumerBuilder>();
                stubMessageConsumerBuilder
                    .Setup(x => x.Build(It.IsAny<ServerName>()))
                    .Returns(stubConsumer.Object);

                var moqSerializerManager = new Mock<IBusSerializerManager>();
                moqSerializerManager
                    .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)))
                    .Returns(JsonMessageSerializer.Instance);

                var (moqLogger, moqMediator) = CreateLoggerMock<FakeMessage>(BusName.Kafka);

                var sut = new KafkaTopicMessageConsumer(stubMessageConsumerBuilder.Object, serviceProvider, stubCache.Object, serviceProvider.GetRequiredService<IEventMiddlewareExecutorProvider>(), moqSerializerManager.Object, moqLogger.Object);

                _ = Task.Run(() => sut.ConsumeUntilCancellationIsRequested(ServerName.Default, FakeMessage.TOPIC_NAME, cancellationTokenSource.Token));
                Thread.Sleep(500);
                cancellationTokenSource.Cancel();

                moqSerializerManager
                    .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakeMessage)), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.IsAny<BusExceptionMessage>(), It.IsAny<CancellationToken>()), Times.Once);

                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => obj.GetType().Equals(typeof(BusTypedExceptionMessage<FakeMessage>))), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        private static ServiceProvider BuildServiceProvider(IMediator mediator)
        {
            var serviceCollection = new ServiceCollection();
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeMessage>>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(MediatorPublisherMiddleware<FakeMessage>) }, typeof(FakeMessage)));
            serviceCollection.AddScoped(s => mediator);
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ServiceProvider BuildServiceProviderWithMiddleware<TEventType>(IMediator mediator, Action<TEventType> onMiddlewareExecuted)
            where TEventType : IEvent
        {
            var stubMiddlewareManager = new Mock<IEventMiddlewareExecutorProvider>();
            stubMiddlewareManager.Setup(x => x.GetPipeline(It.IsAny<BusName>(), It.IsAny<Enum>(), It.IsAny<Type>())).Returns(new Pipeline(new List<Type> { typeof(FakeMessageMiddlewareWithFuncOnConstructor), typeof(MediatorPublisherMiddleware<FakeMessage>) }, typeof(FakeMessage)));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_ => new FakeMessageMiddlewareWithFuncOnConstructor(context =>
            {
                onMiddlewareExecuted((context as ConsumeContext<TEventType>).Message);
                return Task.CompletedTask;
            }));
            serviceCollection.AddSingleton<MediatorPublisherMiddleware<FakeMessage>>();
            serviceCollection.AddSingleton(s => stubMiddlewareManager.Object);
            serviceCollection.AddScoped(s => mediator);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeResult(FakeMessage fakeMessage, bool nullHeader = false)
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = JsonSerializer.SerializeToUtf8Bytes(fakeMessage),
                    Headers = !nullHeader ? new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(fakeMessage.GetType().AssemblyQualifiedName)}
                    } : null
                }
            };
        }

        private static ConsumeResult<string, byte[]> BuildFakeConsumeWithInvalidMessageResult()
        {
            return new ConsumeResult<string, byte[]>
            {
                Message = new Message<string, byte[]>
                {
                    Value = Encoding.UTF8.GetBytes("{"),
                    Headers = new Headers
                    {
                        {"message-type", Encoding.UTF8.GetBytes(typeof(FakeMessage).AssemblyQualifiedName)}
                    }
                }
            };
        }
    }
}