using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineMessage : IEvent
    {
        public const string TOPIC_NAME = "fake-pipeline-topic";

        public string LastMiddlewareToRun { get; }
        public List<string> MiddlewareNames { get; } = new List<string>();

        public FakePipelineMessage()
        {

        }

        public FakePipelineMessage(string lastMiddlewareToRun)
        {
            LastMiddlewareToRun = lastMiddlewareToRun;
        }
    }
}
