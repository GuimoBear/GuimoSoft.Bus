using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageMiddleware : IEventMiddleware<FakeMessage>
    {
        public async Task InvokeAsync(ConsumeContext<FakeMessage> message, Func<Task> next)
        {
            await next();
        }
    }
}
