using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class SecondFakeMessage : IEvent
    {
        public const string TOPIC_NAME = "fake-message";

        public SecondFakeMessage(string key, string someProperty)
        {
            Key = key;
            SomeProperty = someProperty;
        }

        public string Key { get; set; }

        public string SomeProperty { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FakeMessage message)
            {
                return string.Equals(Key, message.Key) &&
                       string.Equals(SomeProperty, message.SomeProperty);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return string.GetHashCode(Key) +
                   string.GetHashCode(SomeProperty);
        }
    }
}