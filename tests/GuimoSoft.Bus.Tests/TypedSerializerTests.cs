using FluentAssertions;
using System.Text.Json;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class TypedSerializerTests
    {
        [Fact]
        public void When_Serialize_Then_NotThrowAnyException()
        {
            var fakeMessage = new OtherFakeMessage("test", "115");

            var expected = JsonSerializer.SerializeToUtf8Bytes(fakeMessage);

            var actual = OtherFakeMessageSerializer.Instance.Serialize(fakeMessage);

            actual
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void When_Deserialize_Then_NotThrowAnyException()
        {
            var expected = new OtherFakeMessage("test", "115");

            var serializedContent = JsonSerializer.SerializeToUtf8Bytes(expected);

            var actual = OtherFakeMessageSerializer.Instance.Deserialize(typeof(OtherFakeMessage), serializedContent);

            actual
                .Should().BeEquivalentTo(expected);
        }
    }
}
