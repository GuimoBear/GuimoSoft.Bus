using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    internal class FakeMessageMiddlewareWithFuncOnConstructor : IEventMiddleware<FakeMessage>
    {
        private readonly Func<ConsumeContext<FakeMessage>, Task> _func;

        public FakeMessageMiddlewareWithFuncOnConstructor(Func<ConsumeContext<FakeMessage>, Task> func)
        {
            _func = func;
        }

        public async Task InvokeAsync(ConsumeContext<FakeMessage> message, Func<Task> next)
        {
            await _func(message);
            await next();
        }
    }
}
