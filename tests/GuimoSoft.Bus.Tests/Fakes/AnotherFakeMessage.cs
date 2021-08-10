using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class AnotherFakeMessage : IEvent
    {
        public const string TOPIC_NAME = "another-fake-message";

        public AnotherFakeMessage(string key, string someAnotherProperty)
        {
            Key = key;
            SomeAnotherProperty = someAnotherProperty;
        }

        public string Key { get; set; }

        public string SomeAnotherProperty { get; set; }
    }
}