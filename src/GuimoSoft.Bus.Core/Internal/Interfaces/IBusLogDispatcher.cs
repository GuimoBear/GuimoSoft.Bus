using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;

namespace GuimoSoft.Bus.Core.Internal.Interfaces
{
    internal interface IBusLogDispatcher
    {
        ISwitchStage FromBus(BusName bus);
    }
}
