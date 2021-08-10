using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeMessageThrowExceptionMiddleware : IEventMiddleware<FakeMessage>
    {
        public Task InvokeAsync(ConsumeContext<FakeMessage> message, Func<Task> next)
        {
            throw new Exception();
        }
    }
}
