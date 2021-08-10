using System;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    public abstract class TypedSerializer<TEvent> : IDefaultSerializer
    {
        protected abstract byte[] Serialize(TEvent message);

        protected abstract TEvent Deserialize(byte[] content);

        public byte[] Serialize(object message)
            => Serialize((TEvent)message);

        public object Deserialize(Type messageType, byte[] content)
            => Deserialize(content);
    }
}
