using System;

namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface ISwitchStage
    {
        IFinalityStage AndSwitch(Enum @switch);
    }
}
