using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core.Interfaces
{
    public interface IBusSerializerManager
    {
        IDefaultSerializer GetSerializer(BusName busName, Finality finality, Enum @switch, Type messageType);
    }
}
