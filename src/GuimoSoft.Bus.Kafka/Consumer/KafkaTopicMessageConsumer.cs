using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal class KafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
    {
        private readonly IKafkaConsumerBuilder _kafkaConsumerBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageTypeCache _cache;
        private readonly IEventMiddlewareExecutorProvider _middlewareManager;
        private readonly IBusSerializerManager _busSerializerManager;
        private readonly IBusLogDispatcher _log;

        private IEnumerable<MessageTypeResolver> _messageTypeResolvers;

        public KafkaTopicMessageConsumer(IKafkaConsumerBuilder kafkaConsumerBuilder, IServiceProvider serviceProvider, IMessageTypeCache cache, IEventMiddlewareExecutorProvider middlewareManager, IBusSerializerManager busSerializerManager, IBusLogDispatcher log)
        {
            _kafkaConsumerBuilder = kafkaConsumerBuilder ?? throw new ArgumentNullException(nameof(kafkaConsumerBuilder));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _middlewareManager = middlewareManager ?? throw new ArgumentNullException(nameof(middlewareManager));
            _busSerializerManager = busSerializerManager ?? throw new ArgumentNullException(nameof(busSerializerManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void ConsumeUntilCancellationIsRequested(Enum @switch, string topic, CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerBuilder.Build(@switch);
            InitializeCachedObjects(@switch, topic);
            consumer.Subscribe(topic);
            ListenUntilCancellationIsRequested(@switch, ref topic, consumer, ref cancellationToken);
            consumer.Close();
        }

        private void ListenUntilCancellationIsRequested(Enum @switch, ref string topic, IConsumer<string, byte[]> consumer, ref CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    ProcessMessage(@switch, ref topic, consumeResult, cancellationToken);
                }
                catch (Exception ex)
                {
                    _log
                        .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                        .WhileListening().TheEndpoint(topic)
                        .Write().Message($"Houve um erro ao consumir a mensagem do tópico {topic}")
                        .With(ex is OperationCanceledException ? BusLogLevel.Warning : BusLogLevel.Error)
                        .Publish().AnException(ex, cancellationToken);
                    if (ex is OperationCanceledException)
                        break;
                }
            }
        }

        private void InitializeCachedObjects(Enum @switch, string topic)
        {
            var messageTypes = _cache.Get(BusName.Kafka, Finality.Consume, @switch, topic);
            var messageTypeResolvers = new List<MessageTypeResolver>();
            foreach (var messageType in messageTypes)
            {
                var serializer = _busSerializerManager.GetSerializer(BusName.Kafka, Finality.Consume, @switch, messageType);
                var pipeline = _middlewareManager.GetPipeline(BusName.Kafka, @switch, messageType);
                messageTypeResolvers.Add(new(messageType, serializer, pipeline));
            }
            _messageTypeResolvers = messageTypeResolvers;
        }

        private void ProcessMessage(Enum @switch, ref string topic, ConsumeResult<string, byte[]> consumeResult, CancellationToken cancellationToken)
        {
            foreach (var messageTypeResolver in _messageTypeResolvers)
                ProcessMessageType(@switch, topic, consumeResult, messageTypeResolver, cancellationToken);
        }

        private void ProcessMessageType(Enum @switch, string topic, ConsumeResult<string, byte[]> consumeResult, MessageTypeResolver messageTypeResolver, CancellationToken cancellationToken)
        {
            try
            {
                if (!TryDeserialize(messageTypeResolver, consumeResult.Message.Value, out var deserializedMessage))
                {
                    _log
                        .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                        .AfterReceived().TheObject(messageTypeResolver.MessageType, deserializedMessage).FromEndpoint(topic)
                        .Write().Message($"Houve um erro ao deserializar a mensagem do tipo {messageTypeResolver.MessageType.Name} após receber uma mensagem do tópico {topic}")
                        .With(BusLogLevel.Error)
                        .Publish().AnLog(cancellationToken);
                    return;
                }

                ExecutePipeline(@switch, topic, consumeResult.Message.Headers, messageTypeResolver, deserializedMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _log
                    .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                    .WhileListening().TheEndpoint(topic)
                    .Write().Message($"Houve um erro ao deserializar a mensagem do tipo {messageTypeResolver.MessageType.Name} após receber uma mensagem do tópico {topic}")
                    .With(BusLogLevel.Error)
                    .Publish().AnException(ex, cancellationToken);
            }
        }

        private static bool TryDeserialize(MessageTypeResolver messageTypeResolver, byte[] content, out object deserializedMessage)
        {
            deserializedMessage = messageTypeResolver.Serializer.Deserialize(messageTypeResolver.MessageType, content);
            return deserializedMessage is not null;
        }

        private void ExecutePipeline(Enum @switch, string topic, Headers headers, MessageTypeResolver messageTypeResolver, object deserializedMessage, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var informations = GetInformations(@switch, ref topic, headers);

                messageTypeResolver.Pipeline.Execute(deserializedMessage, scope.ServiceProvider, informations, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log
                    .FromBus(BusName.Kafka).AndSwitch(@switch).AndFinality(Finality.Consume)
                    .AfterReceived().TheObject(deserializedMessage).FromEndpoint(topic)
                    .Write().Message($"Houve um erro ao executar a pipeline do tipo {messageTypeResolver.MessageType.Name} após receber uma mensagem do tópico {topic}")
                    .With(ex is OperationCanceledException ? BusLogLevel.Warning : BusLogLevel.Error)
                    .Publish().AnException(ex, cancellationToken);
            }
        }

        private static ConsumeInformations GetInformations(Enum @switch, ref string topic, Headers headers)
        {
            var informations = new ConsumeInformations(BusName.Kafka, @switch, topic);
            headers?.Select(header => new KeyValuePair<string, string>(header.Key, Encoding.UTF8.GetString(header.GetValueBytes())))
                    .ToList()
                    .ForEach(header => informations.AddHeader(header.Key, header.Value));
            return informations;
        }

        private class MessageTypeResolver
        {
            public Type MessageType { get; }
            public IDefaultSerializer Serializer { get; }
            public Pipeline Pipeline { get; }

            public MessageTypeResolver(Type messageType, IDefaultSerializer serializer, Pipeline pipeline)
            {
                MessageType = messageType;
                Serializer = serializer;
                Pipeline = pipeline;
            }
        }
    }
}