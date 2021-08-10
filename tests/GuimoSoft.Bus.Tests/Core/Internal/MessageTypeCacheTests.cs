using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class MessageTypeCacheTests
    {
        [Fact]
        public void AddShouldBeThrowArgumentExceptionIfMessageNotImplementIMessage()
        {
            var sut = new MessageTypeCache();
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(MessageTypeCacheTests), "test"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AddShouldBeThrowArgumentExceptionIfEndpointIsNullEmptyOrWritespace(string endpoint)
        {
            var sut = new MessageTypeCache();
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), endpoint));
        }

        [Fact]
        public void AddFacts()
        {
            var sut = new MessageTypeCache();
            Assert.Throws<ArgumentNullException>(() => sut.Add(BusName.Kafka, Finality.Produce, null, typeof(FakeMessage), "test"));

            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), null));
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), ""));
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "  "));

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "test");

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "test 2");

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "test");

            sut.Get(typeof(FakeMessage))
                .ToList().Should().HaveCount(2);
        }

        [Fact]
        public void GetSwitchersFacts()
        {
            var sut = new MessageTypeCache();
            Assert.Throws<InvalidOperationException>(() => sut.GetSwitchers(BusName.Kafka, Finality.Produce));

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "test");

            Assert.Throws<InvalidOperationException>(() => sut.GetSwitchers(BusName.Kafka, Finality.Consume));

            sut.GetSwitchers(BusName.Kafka, Finality.Produce)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetEndpointsFacts()
        {
            var sut = new MessageTypeCache();
            Assert.Throws<ArgumentNullException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, null));

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, ServerName.Default));

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Consume, ServerName.Default));

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1));

            sut.GetEndpoints(BusName.Kafka, Finality.Produce, ServerName.Default)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetByMessageInstanceFacts()
        {
            var fakeMessage = new FakeMessage("test", "test");
            var anotherFakeMessage = new AnotherFakeMessage("g", "sd");
            var sut = new MessageTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, null, fakeMessage));

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, null as Type));

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, null as FakeMessage));

            sut.Add(BusName.Kafka, Finality.Consume, FakeServerName.FakeHost1, typeof(FakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, fakeMessage));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeMessage));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(AnotherFakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeMessage));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeMessage), "test");

            sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeMessage)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetByEndpointFacts()
        {
            var sut = new MessageTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, null, "test"));

            Assert.Throws<ArgumentException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, null as string));

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeMessage.TOPIC_NAME));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(AnotherFakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, FakeMessage.TOPIC_NAME));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeMessage.TOPIC_NAME));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeMessage), FakeMessage.TOPIC_NAME);

            sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeMessage.TOPIC_NAME)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetBymessageTypeFacts()
        {
            var sut = new MessageTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(null));

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeMessage)));

            sut.Add(BusName.Kafka, Finality.Consume, FakeServerName.FakeHost1, typeof(FakeMessage), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeMessage)));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeMessage), "test");

            sut.Get(typeof(FakeMessage))
                .Should().HaveCount(1);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeMessage), "test");

            sut.Get(typeof(FakeMessage))
                .Should().HaveCount(2);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeMessage), "test");

            sut.Get(typeof(FakeMessage))
                .ToList().Should().HaveCount(2);

            new MessageTypeCache.MessageTypeItem(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeMessage), "test")
                .Equals(null)
                .Should().BeFalse();
        }
    }
}
