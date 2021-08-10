using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IFinalityStage
    {
        IListeningStage AndFinality(Finality finality);
    }
}
