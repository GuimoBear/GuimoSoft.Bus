using FluentAssertions;
using System;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core
{
    public class MessageSerializerManagerTests
    {
        [Fact]
        public void When_SetDefaultSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MessageSerializerManager.Instance.SetDefaultSerializer(null));
        }

        [Fact]
        public void When_SetDefaultSerializerWithAnValidSerializer_Then_SetSerializer()
        {
            lock (Utils.Lock)
            { 
                MessageSerializerManager.Instance.GetSerializer(typeof(FakeMessage))
                    .Should().BeSameAs(JsonMessageSerializer.Instance);

                MessageSerializerManager.Instance.GetSerializer(typeof(OtherFakeMessage))
                    .Should().BeSameAs(JsonMessageSerializer.Instance);

                MessageSerializerManager.Instance.SetDefaultSerializer(FakeDefaultSerializer.Instance);

                MessageSerializerManager.Instance.GetSerializer(typeof(FakeMessage))
                    .Should().BeSameAs(FakeDefaultSerializer.Instance);

                MessageSerializerManager.Instance.GetSerializer(typeof(FakeMessage))
                    .Should().BeSameAs(FakeDefaultSerializer.Instance);

                MessageSerializerManager.Instance.SetDefaultSerializer(JsonMessageSerializer.Instance);

                MessageSerializerManager.Instance.GetSerializer(typeof(FakeMessage))
                    .Should().BeSameAs(JsonMessageSerializer.Instance);

                MessageSerializerManager.Instance.GetSerializer(typeof(OtherFakeMessage))
                    .Should().BeSameAs(JsonMessageSerializer.Instance);

                Utils.ResetarMessageSerializerManager();
            }
        }

        [Fact]
        public void When_AddTypedSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MessageSerializerManager.Instance.AddTypedSerializer<FakeMessage>(null));
        }

        [Fact]
        public void When_AddTypedSerializerWithAnValidTypedSerializer_Then_SetTypedSerializer()
        {
            lock (Utils.Lock)
            {
                MessageSerializerManager.Instance.GetSerializer(typeof(OtherFakeMessage))
                    .Should().BeSameAs(JsonMessageSerializer.Instance);

               MessageSerializerManager.Instance.AddTypedSerializer(OtherFakeMessageSerializer.Instance);

               MessageSerializerManager.Instance.GetSerializer(typeof(OtherFakeMessage))
                    .Should().BeSameAs(OtherFakeMessageSerializer.Instance);

                Utils.ResetarMessageSerializerManager();
            }
        }
    }
}
