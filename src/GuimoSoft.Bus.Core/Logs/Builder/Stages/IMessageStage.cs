namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IMessageStage
    {
        ILogLevelAndDataStage Message(string message);
    }
}
