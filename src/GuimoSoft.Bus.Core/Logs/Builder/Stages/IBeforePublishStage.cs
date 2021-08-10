namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IBeforePublishStage
    {
        IPublishStage Publish();
    }
}
