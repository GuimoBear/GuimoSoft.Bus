using System;
using System.Text.Json.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Benchmarks.Handlers.Benchmark
{
    public class BenchmarkMessage : IEvent
    {
        public const string TOPIC_NAME = "benchmark-message-topic";

        [JsonPropertyName(nameof(Id))]
        public Guid Id { get; private set; }

        [JsonConstructor]
        public BenchmarkMessage(Guid id)
        {
            Id = id;
        }
    }
}
