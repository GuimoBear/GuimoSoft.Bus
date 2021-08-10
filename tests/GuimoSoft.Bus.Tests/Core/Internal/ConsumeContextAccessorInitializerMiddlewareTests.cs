using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class ConsumeContextAccessorInitializerMiddlewareTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConsumeContextAccessorInitializerMiddleware<FakeMessage>(null));
        }

        [Fact]
        public async Task InvokeAsyncFacts()
        {
            var expectedContext = new ConsumeContext<FakeMessage>(new FakeMessage("", ""), default, default, CancellationToken.None);

            var contextAccessor = new ConsumeContextAccessor<FakeMessage>();

            contextAccessor.Context
                .Should().BeNull();

            Func<Task> checkContext = () =>
            {
                contextAccessor.Context
                    .Should().BeSameAs(expectedContext);
                return Task.CompletedTask;
            };

            var sut = new ConsumeContextAccessorInitializerMiddleware<FakeMessage>(contextAccessor);

            await sut.InvokeAsync(expectedContext, checkContext);

            contextAccessor.Context
                .Should().BeNull();
        }
    }
}
