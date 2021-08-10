namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IListeningStage
    {
        IEndpointStage WhileListening();
        IMessageObjectInstance AfterReceived();
        IMessageStage Write();
    }
}