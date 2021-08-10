namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IKeyValueStage
    {
        ILogLevelAndDataStage FromValue(object value);
    }
}
