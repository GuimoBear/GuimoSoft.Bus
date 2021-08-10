using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public class PipelineTests
    {

        private static readonly IReadOnlyDictionary<string, string> EMPTY_HEADER = new Dictionary<string, string>();

        private readonly IServiceProvider services;
        private readonly Pipeline pipeline;

        public PipelineTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareOne>();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareTwo>();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareThree>();
            services = serviceCollection.BuildServiceProvider();
            var middlewareTypes = new List<Type>
            {
                typeof(FakePipelineMessageMiddlewareOne),
                typeof(FakePipelineMessageMiddlewareTwo),
                typeof(FakePipelineMessageMiddlewareThree)
            };
            pipeline = new Pipeline(middlewareTypes, typeof(FakePipelineMessage));
        }

        [Fact]
        public void ConstructiorWithInvalidParametersShouldBeThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Pipeline(null, default));
            Assert.Throws<ArgumentException>(() => new Pipeline(new List<Type>(), typeof(PipelineTests)));

            var middlewareTypes = new List<Type>
            {
                typeof(FakePipelineMessageMiddlewareOne),
                typeof(FakePipelineMessageMiddlewareTwo),
                typeof(FakePipelineMessageMiddlewareThree),
                typeof(FakeMessageMiddleware)
            };
            Assert.Throws<ArgumentException>(() => new Pipeline(middlewareTypes, typeof(FakePipelineMessage)));
        }

        [Fact]
        public async Task ExecuteShouldBeExecutedWithoutExceptions()
        {
            var message = new FakePipelineMessage();
            using var scope = services.CreateScope();
            var context = await pipeline.Execute(message, scope.ServiceProvider, new ConsumeInformations(BusName.Kafka, FakeServerName.FakeHost2, "e"), CancellationToken.None) as ConsumeContext<FakePipelineMessage>;

            context
                .Should().NotBeNull();

            context.Items
                .Should().NotBeNull().And.HaveCount(3);

            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareOne.Name);
            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareTwo.Name);
            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareThree.Name);

            context.Message
                .Should().NotBeNull();

            context.Message.MiddlewareNames
                .Should().NotBeNull().And.HaveCount(3);

            context.Message.MiddlewareNames[0]
                .Should().Be(FakePipelineMessageMiddlewareOne.Name);

            context.Message.MiddlewareNames[1]
                .Should().Be(FakePipelineMessageMiddlewareTwo.Name);
            context.Message.MiddlewareNames[2]
                .Should().Be(FakePipelineMessageMiddlewareThree.Name);
        }


        [Fact]
        public async Task ExecuteWithExecutionStopInMiddlewareTwoShouldBeExecutedWithoutExceptions()
        {
            var message = new FakePipelineMessage(FakePipelineMessageMiddlewareTwo.Name);
            using var scope = services.CreateScope();

            var context = await pipeline.Execute(message, scope.ServiceProvider, new ConsumeInformations(BusName.Kafka, ServerName.Default, "e"), CancellationToken.None) as ConsumeContext<FakePipelineMessage>;

            message
                .MiddlewareNames.Should().NotBeNull().And.HaveCount(2);

            message.MiddlewareNames[0]
                .Should().Be(FakePipelineMessageMiddlewareOne.Name);

            message.MiddlewareNames[1]
                .Should().Be(FakePipelineMessageMiddlewareTwo.Name);
        }
    }
}
