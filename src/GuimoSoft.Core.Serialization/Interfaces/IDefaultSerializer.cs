using System;

namespace GuimoSoft.Core.Serialization.Interfaces
{
    public interface IDefaultSerializer
    {
        byte[] Serialize(object message);

        object Deserialize(Type messageType, byte[] content);
    }
}
