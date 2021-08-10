using System;
using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    internal sealed class JsonMessageSerializer : IDefaultSerializer
    {
        public static readonly IDefaultSerializer Instance = new JsonMessageSerializer();

        private JsonMessageSerializer() { }

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
