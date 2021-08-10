using Confluent.Kafka;
using FluentAssertions;
using System;
using System.Collections;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class BusOptionsDictionaryTests
    {
        [Fact]
        public void DictionaryFacts()
        {
            var sut = new BusOptionsDictionary<ConsumerConfig>();

            Assert.Throws<KeyNotFoundException>(() => sut[ServerName.Default]);
            Assert.Throws<ArgumentNullException>(() => sut[ServerName.Default] = null);

            CheckLength(sut, 0);

            Assert.Throws<ArgumentNullException>(() => sut.Add(ServerName.Default, null));

            sut.Add(ServerName.Default, new ConsumerConfig());

            Assert.Throws<ArgumentException>(() => sut.Add(ServerName.Default, new ConsumerConfig()));

            sut[ServerName.Default]
                .Should().NotBeNull();

            CheckLength(sut, 1);

            sut.TryGetValue(ServerName.Default, out var options);

            options
                .Should().NotBeNull();

            sut.ContainsKey(ServerName.Default)
                .Should().BeTrue();

            sut.Remove(ServerName.Default)
                .Should().BeTrue();

            sut.Clear();

            CheckLength(sut, 0);

            sut.TryGetValue(ServerName.Default, out options);

            options
                .Should().BeNull();

            (sut as IEnumerable).GetEnumerator();
        }

        private static void CheckLength(BusOptionsDictionary<ConsumerConfig> sut, int expectedLength)
        {
            sut.Keys
                .Should().HaveCount(expectedLength);

            sut.Values
                .Should().HaveCount(expectedLength);

            sut.Count
                .Should().Be(expectedLength);
        }
    }
}
