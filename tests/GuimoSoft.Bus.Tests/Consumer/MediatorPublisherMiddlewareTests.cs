using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class MediatorPublisherMiddlewareTests
    {
        private static readonly IReadOnlyDictionary<string, string> EMPTY_HEADERS = new Dictionary<string, string>();

        [Fact]
        public async Task When_ExecuteWithoutMediatRConfigured_Then_ThrowException()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var message = new FakeMessage("test", "");
            var context = new ConsumeContext<FakeMessage>(message, serviceProvider, new ConsumeInformations(BusName.Kafka, ServerName.Default, "e"), CancellationToken.None);

            var sut = new MediatorPublisherMiddleware<FakeMessage>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(context, () => Task.CompletedTask));
        }

        [Fact]
        public async Task When_ExecuteWithMediatRConfigured_Then_CallsMediatRPublishMethod()
        {
            var services = new ServiceCollection();

            var message = new FakeMessage("test", "");

            var moqMediatr = new Mock<IMediator>();

            services.AddSingleton(moqMediatr.Object);

            var serviceProvider = services.BuildServiceProvider();
            var context = new ConsumeContext<FakeMessage>(message, serviceProvider, new ConsumeInformations(BusName.Kafka, ServerName.Default, "e"), CancellationToken.None);

            var sut = new MediatorPublisherMiddleware<FakeMessage>();

            await sut.InvokeAsync(context, () => Task.CompletedTask);

            moqMediatr.Verify(x => x.Publish(message, It.IsAny<CancellationToken>()), Times.Once);

            context.GetMessage()
                .Should().BeEquivalentTo(message);
        }
    }
}
