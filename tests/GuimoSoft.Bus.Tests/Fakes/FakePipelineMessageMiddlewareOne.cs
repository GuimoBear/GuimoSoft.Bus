using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineMessageMiddlewareOne : IEventMiddleware<FakePipelineMessage>
    {
        public const string Name = nameof(FakePipelineMessageMiddlewareOne);

        public async Task InvokeAsync(ConsumeContext<FakePipelineMessage> context, Func<Task> next)
        {
            Console.WriteLine(context.Informations.Bus);
            Console.WriteLine(context.Informations.Switch);
            Console.WriteLine(context.Informations.Endpoint);
            Console.WriteLine(string.Join(", ", context.Informations.Headers.Select(header => $"{header.Key} - {header.Value}")));

            context.Message.MiddlewareNames.Add(Name);
            context.Items.Add(Name, true);
            if (Name.Equals(context.Message.LastMiddlewareToRun))
                return;
            await next();
        }
    }
}
