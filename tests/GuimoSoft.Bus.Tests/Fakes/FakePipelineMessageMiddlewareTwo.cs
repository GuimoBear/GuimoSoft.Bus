using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineMessageMiddlewareTwo : IEventMiddleware<FakePipelineMessage>
    {
        public const string Name = nameof(FakePipelineMessageMiddlewareTwo);

        public async Task InvokeAsync(ConsumeContext<FakePipelineMessage> context, Func<Task> next)
        {
            context.Message.MiddlewareNames.Add(Name);
            context.Items.Add(Name, true);
            if (Name.Equals(context.Message.LastMiddlewareToRun))
                return;
            await next();
        }
    }
}
