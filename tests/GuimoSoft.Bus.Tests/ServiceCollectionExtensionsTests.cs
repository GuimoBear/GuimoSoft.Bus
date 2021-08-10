using Confluent.Kafka;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionExtensionsWithDefaultConfigFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var services = new ServiceCollection();

                services
                    .AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            })
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Consume()
                                .OfType<FakeMessage>()
                                .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<FakePipelineMessage>()
                                .WithMiddleware<FakePipelineMessageMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineMessage.TOPIC_NAME);
                    })
                    .AddKafkaProducer(configurer =>
                    {
                        configurer
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            })
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME);
                    });

                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaMessageConsumerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IConsumeContextAccessor<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogDispatcher)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(ConsumeContextAccessorInitializerMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(MediatorPublisherMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaConsumerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicMessageConsumer)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ImplementationType == typeof(KafkaConsumerMessageHandler)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusSerializerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareExecutorProvider)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareRegister)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageTypeCache)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ConsumerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ProducerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventDispatcher)).Should().NotBeNull();

                var messageMiddlewareServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareManager));

                messageMiddlewareServiceDescriptor
                    .Should().NotBeNull();

                var messageMiddleware = messageMiddlewareServiceDescriptor.ImplementationInstance as EventMiddlewareManager;

                messageMiddleware
                    .Should().NotBeNull();

                messageMiddleware
                    .messageMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeMessage)));

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeMessage))]
                    .Should().NotBeNullOrEmpty();

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, Bus.Abstractions.ServerName.Default, typeof(FakeMessage))]
                    .First()
                    .Should().Be(typeof(FakeMessageMiddleware));

                var sp = services.BuildServiceProvider(true);

                sp.GetRequiredService<IEventDispatcher>();
                sp.GetRequiredService<IKafkaTopicMessageConsumer>();
            }
        }

        [Fact]
        public void ServiceCollectionExtensionsWithSwitcheableConfigFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var services = new ServiceCollection();

                services
                    .AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware(_ => new FakeMessageMiddleware())
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);

                    })
                    .AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME);
                    });

                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaMessageConsumerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IConsumeContextAccessor<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogDispatcher)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(ConsumeContextAccessorInitializerMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(MediatorPublisherMiddleware<>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaConsumerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaTopicMessageConsumer)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ImplementationType == typeof(KafkaConsumerMessageHandler)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusSerializerManager)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareExecutorProvider)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareRegister)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMessageTypeCache)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ConsumerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IBusOptionsDictionary<ProducerConfig>)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaProducerBuilder)).Should().NotBeNull();
                services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventDispatcher)).Should().NotBeNull();

                var messageMiddlewareServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMiddlewareManager));

                messageMiddlewareServiceDescriptor
                    .Should().NotBeNull();

                var messageMiddleware = messageMiddlewareServiceDescriptor.ImplementationInstance as EventMiddlewareManager;

                messageMiddleware
                    .Should().NotBeNull();

                messageMiddleware
                    .messageMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, ServerName.Host1, typeof(FakeMessage)));

                messageMiddleware
                    .messageMiddlewareTypes
                    .Should().ContainKey((BusName.Kafka, ServerName.Host2, typeof(FakeMessage)));

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, ServerName.Host1, typeof(FakeMessage))]
                    .Should().NotBeNullOrEmpty();

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, ServerName.Host2, typeof(FakeMessage))]
                    .Should().NotBeNullOrEmpty();

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, ServerName.Host1, typeof(FakeMessage))]
                    .First()
                    .Should().Be(typeof(FakeMessageMiddleware));

                messageMiddleware
                    .messageMiddlewareTypes[(BusName.Kafka, ServerName.Host2, typeof(FakeMessage))]
                    .First()
                    .Should().Be(typeof(FakeMessageMiddleware));

                var sp = services.BuildServiceProvider(true);

                sp.GetRequiredService<IEventDispatcher>();
                sp.GetRequiredService<IKafkaTopicMessageConsumer>();
            }
        }

        [Fact]
        public void BusOptionsMissingOnConsumerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Consume()
                                .OfType<FakeMessage>()
                                .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<FakePipelineMessage>()
                                .WithMiddleware<FakePipelineMessageMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineMessage.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware(_ => new FakeMessageMiddleware())
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void BusOptionsMissingOnProducerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusOptionsMissingException>(() =>
                {
                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME);
                    });
                });
                Utils.ResetarSingletons();
            }
        }



















        [Fact]
        public void BusAlreadyConfiguredExceptionOnConsumerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            })
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Consume()
                                .OfType<FakeMessage>()
                                .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Consume()
                                .OfType<FakePipelineMessage>()
                                .WithMiddleware<FakePipelineMessageMiddlewareOne>(ServiceLifetime.Scoped)
                                .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo(), ServiceLifetime.Singleton)
                                .FromEndpoint(FakePipelineMessage.TOPIC_NAME);
                    });

                    services.AddKafkaConsumer(configurer =>
                    {
                        configurer
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            });
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware(_ => new FakeMessageMiddleware())
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                })
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Consume()
                                    .OfType<FakeMessage>()
                                    .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                    .FromEndpoint(FakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .FromEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .Consume()
                                    .OfType<FakePipelineMessage>()
                                    .WithMiddleware<FakePipelineMessageMiddlewareOne>()
                                    .WithMiddleware(_ => new FakePipelineMessageMiddlewareTwo())
                                    .FromEndpoint(FakePipelineMessage.TOPIC_NAME);
                    });

                    services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                });

                        switcher
                            .When(ServerName.Host2)
                                .FromServer(options =>
                                {
                                    options.GroupId = "test";
                                    options.BootstrapServers = "localhost";
                                });
                    });
                });

                services.AddKafkaConsumerSwitcher<ServerName>(switcher =>
                {
                    switcher
                        .When(ServerName.Host3)
                            .Consume()
                                .OfType<FakeMessage>()
                                .WithMiddleware<FakeMessageMiddleware>(ServiceLifetime.Transient)
                                .FromEndpoint(FakeMessage.TOPIC_NAME)
                            .FromServer(options =>
                            {
                                options.GroupId = "test";
                                options.BootstrapServers = "localhost";
                            });
                });

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void BusAlreadyConfiguredExceptionOnProducerFacts()
        {
            lock (Utils.Lock)
            {
                var services = new ServiceCollection();

                Utils.ResetarSingletons();
                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .Produce()
                                .FromType<OtherFakeMessage>()
                                .WithSerializer(OtherFakeMessageSerializer.Instance)
                                .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                    });

                    services.AddKafkaProducer(configurer =>
                    {
                        configurer
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                    });
                });
                Utils.ResetarSingletons();

                Assert.Throws<BusAlreadyConfiguredException>(() =>
                {
                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });

                        switcher
                            .When(ServerName.Host2)
                                .WithDefaultSerializer(FakeDefaultSerializer.Instance)
                                .Produce()
                                    .FromType<FakeMessage>()
                                    .ToEndpoint(FakeMessage.TOPIC_NAME)
                                .Produce()
                                    .FromType<OtherFakeMessage>()
                                    .WithSerializer(OtherFakeMessageSerializer.Instance)
                                    .ToEndpoint(OtherFakeMessage.TOPIC_NAME)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });
                    });

                    services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                    {
                        switcher
                            .When(ServerName.Host1)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });

                        switcher
                            .When(ServerName.Host2)
                                .ToServer(options =>
                                {
                                    options.BootstrapServers = "localhost";
                                    options.Acks = Acks.All;
                                });
                    });
                });

                services.AddKafkaProducerSwitcher<ServerName>(switcher =>
                {
                    switcher
                        .When(ServerName.Host3)
                            .Produce()
                                .FromType<FakeMessage>()
                                .ToEndpoint(FakeMessage.TOPIC_NAME)
                            .ToServer(options =>
                            {
                                options.BootstrapServers = "localhost";
                                options.Acks = Acks.All;
                            });
                });
                Utils.ResetarSingletons();
            }
        }
















        private enum ServerName
        {
            Host1,
            Host2, 
            Host3
        }
    }
}
