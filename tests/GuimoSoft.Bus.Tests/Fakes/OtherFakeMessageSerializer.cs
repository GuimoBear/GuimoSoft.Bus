using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class OtherFakeMessageSerializer : TypedSerializer<OtherFakeMessage>
    {
        public static TypedSerializer<OtherFakeMessage> Instance
            = new OtherFakeMessageSerializer();

        private OtherFakeMessageSerializer() { }

        protected override OtherFakeMessage Deserialize(byte[] content)
        {
            return JsonSerializer.Deserialize<OtherFakeMessage>(Encoding.UTF8.GetString(content));
        }

        protected override byte[] Serialize(OtherFakeMessage message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message);
        }
    }
}
