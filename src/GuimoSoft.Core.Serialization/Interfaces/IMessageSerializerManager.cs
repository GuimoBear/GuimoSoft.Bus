using System;

namespace GuimoSoft.Core.Serialization.Interfaces
{
    internal interface IMessageSerializerManager
    {
        IDefaultSerializer GetSerializer(Type messageType);
    }
}
