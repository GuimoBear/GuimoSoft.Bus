using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;
using GuimoSoft.Bus.Benchmarks.Fakes.Interfaces;

namespace GuimoSoft.Bus.Benchmarks.Fakes.Kafka
{
    public sealed class InMemoryConsumer : IConsumer<string, byte[]>
    {
        private readonly IBrokerConsumer consumer;

        public string MemberId { get; } = "In Memory Kafka member id";

        public List<TopicPartition> Assignment { get; } = new List<TopicPartition>();

        public List<string> Subscription { get; } = new List<string>();

        public IConsumerGroupMetadata ConsumerGroupMetadata { get; } = default;

        public Handle Handle { get; } = new Handle();

        public string Name { get; } = "In Memory Kafka";

        public InMemoryConsumer(IBrokerConsumer consumer)
        {
            if (consumer is null)
                throw new ArgumentNullException(nameof(consumer));
            this.consumer = consumer;
        }

        public int AddBrokers(string brokers) => default;

        public void Assign(TopicPartition partition) { }

        public void Assign(TopicPartitionOffset partition) { }

        public void Assign(IEnumerable<TopicPartitionOffset> partitions) { }

        public void Assign(IEnumerable<TopicPartition> partitions) { }

        public void Close() { }

        public List<TopicPartitionOffset> Commit() => default;

        public void Commit(IEnumerable<TopicPartitionOffset> offsets) { }

        public void Commit(ConsumeResult<string, byte[]> result) { }

        public List<TopicPartitionOffset> Committed(TimeSpan timeout) => default;

        public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout) => default;

        public ConsumeResult<string, byte[]> Consume(int millisecondsTimeout)
        {
            var response = consumer.Consume(Subscription, millisecondsTimeout);
            if (response != default)
            {
                return new ConsumeResult<string, byte[]>
                {
                    Topic = response.topic,
                    Message = new Message<string, byte[]>
                    {
                        Key = response.topic,
                        Value = response.message
                    }
                };
            }
            return default;
        }

        public ConsumeResult<string, byte[]> Consume(CancellationToken cancellationToken = default)
        {
            var response = consumer.Consume(Subscription, cancellationToken);
            if (response != default)
            {
                return new ConsumeResult<string, byte[]>
                {
                    Topic = response.topic,
                    Message = new Message<string, byte[]>
                    {
                        Key = response.topic,
                        Value = response.message
                    }
                };
            }
            return default;
        }

        public ConsumeResult<string, byte[]> Consume(TimeSpan timeout)
        {
            var response = consumer.Consume(Subscription, timeout);
            if (response != default)
            {
                return new ConsumeResult<string, byte[]>
                {
                    Topic = response.topic,
                    Message = new Message<string, byte[]>
                    {
                        Key = response.topic,
                        Value = response.message
                    }
                };
            }
            return default;
        }

        public void Dispose() { }

        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition) => default;

        public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout) => default;

        public void Pause(IEnumerable<TopicPartition> partitions) { }

        public Offset Position(TopicPartition partition) => default;

        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout) => default;

        public void Resume(IEnumerable<TopicPartition> partitions) { }

        public void Seek(TopicPartitionOffset tpo) { }

        public void StoreOffset(ConsumeResult<string, byte[]> result) { }

        public void StoreOffset(TopicPartitionOffset offset) { }

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics is not null)
            {
                foreach (var topic in topics)
                    Subscribe(topic);
            }
        }

        public void Subscribe(string topic)
        {
            if (!Subscription.Contains(topic))
            {
                consumer.CreateTopicIfNotExists(topic);
                Subscription.Add(topic);
            }
        }

        public void Unassign() { }

        public void Unsubscribe() { }

        public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions) { }

        public void IncrementalAssign(IEnumerable<TopicPartition> partitions) { }

        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions) { }
    }
}
