using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class BusSerializerManagerTests
    {
        [Fact]
        public void BusSerializerManagerFacts()
        {
            lock (Utils.Lock)
            {
                var sut = new BusSerializerManager();

                sut.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakePipelineMessage))
                    .Should().BeSameAs(MessageSerializerManager.Instance.GetSerializer(typeof(FakePipelineMessage)));

                sut.AddTypedSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, FakePipelineMessageSerializer.Instance);

                sut.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakePipelineMessage))
                    .Should().BeSameAs(FakePipelineMessageSerializer.Instance);

                Utils.ResetarMessageSerializerManager();
            }
        }
    }
}
