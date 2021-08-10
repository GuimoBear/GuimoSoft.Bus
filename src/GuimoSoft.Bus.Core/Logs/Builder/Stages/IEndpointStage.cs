namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IEndpointStage
    {
        IWriteStage TheEndpoint(string endpoint);
    }
}
