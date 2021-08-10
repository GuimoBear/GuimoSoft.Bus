using System.Text;
using System.Text.Json;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineMessageSerializer : TypedSerializer<FakePipelineMessage>
    {
        public static TypedSerializer<FakePipelineMessage> Instance
               = new FakePipelineMessageSerializer();

        private FakePipelineMessageSerializer() { }

        protected override FakePipelineMessage Deserialize(byte[] content)
        {
            return JsonSerializer.Deserialize<FakePipelineMessage>(Encoding.UTF8.GetString(content));
        }

        protected override byte[] Serialize(FakePipelineMessage message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message);
        }
    }
}
