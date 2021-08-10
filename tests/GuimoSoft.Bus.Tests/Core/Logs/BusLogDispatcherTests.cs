using DeepEqual.Syntax;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs
{
    public class BusLogDispatcherTests
    {
        [Fact]
        public void ConstructorShouldCreateBusLogDispatcher()
        {
            var sut = new BusLogDispatcher(Mock.Of<IMediator>());
            Assert.IsType<BusLogDispatcher>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BusLogDispatcher(null));
        }


        [Fact]
        public void FromBusFacts()
        {
            var mediator = Mock.Of<IMediator>();

            var expectedLogBuilder = new BusLogDispatcherBuilder(mediator, Bus.Abstractions.BusName.Kafka);

            var sut = new BusLogDispatcher(mediator);

            var builder = sut.FromBus(Bus.Abstractions.BusName.Kafka);

            builder
                .Should().BeOfType<BusLogDispatcherBuilder>();

            builder.WithDeepEqual(expectedLogBuilder)
                .Assert();
        }
    }
}
