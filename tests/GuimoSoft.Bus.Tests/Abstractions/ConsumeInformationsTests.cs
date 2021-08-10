using FluentAssertions;
using System;
using GuimoSoft.Bus.Abstractions;
using Xunit;

namespace GuimoSoft.Bus.Tests.Abstractions
{
    public class ConsumeInformationsTests
    {
        [Fact]
        public void AddHeaderFacts()
        {
            var sut = new ConsumeInformations(BusName.Kafka, ServerName.Default, "");

            Assert.Throws<ArgumentException>(() => sut.AddHeader(null, "value"));
            Assert.Throws<ArgumentException>(() => sut.AddHeader("", "value"));
            Assert.Throws<ArgumentException>(() => sut.AddHeader(" ", "value"));

            Assert.Throws<ArgumentException>(() => sut.AddHeader("key", null));
            Assert.Throws<ArgumentException>(() => sut.AddHeader("key", ""));
            Assert.Throws<ArgumentException>(() => sut.AddHeader("key", " "));

            sut.Headers
                .Should().BeEmpty();

            sut.AddHeader("key", "value");

            sut.Headers
                .Should().NotBeEmpty();
        }
    }
}
