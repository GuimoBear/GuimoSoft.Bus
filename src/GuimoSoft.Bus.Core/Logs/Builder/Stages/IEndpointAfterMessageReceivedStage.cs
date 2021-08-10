namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IEndpointAfterMessageReceivedStage
    {
        IWriteStage FromEndpoint(string endpoint);
    }
}
