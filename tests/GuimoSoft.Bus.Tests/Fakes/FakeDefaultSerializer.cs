using System;
using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeDefaultSerializer : IDefaultSerializer
    {
        public static IDefaultSerializer Instance
            = new FakeDefaultSerializer();

        private FakeDefaultSerializer() { }

        public byte[] Serialize(object message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message);
        }

        public object Deserialize(Type messageType, byte[] content)
        {
            var stringContent = Encoding.UTF8.GetString(content);
            return JsonSerializer.Deserialize(stringContent, messageType);
        }
    }
}
