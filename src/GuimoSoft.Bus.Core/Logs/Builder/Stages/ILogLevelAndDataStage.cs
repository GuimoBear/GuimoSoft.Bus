namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface ILogLevelAndDataStage
    {
        IKeyValueStage AndKey(string key);
        IBeforePublishStage With(BusLogLevel level);
    }
}
