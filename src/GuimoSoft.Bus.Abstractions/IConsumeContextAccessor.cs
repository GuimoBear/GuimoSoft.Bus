namespace GuimoSoft.Bus.Abstractions
{
    public interface IConsumeContextAccessor<TEvent> where TEvent : IEvent
    {
        ConsumeContext<TEvent> Context { get; set; }
    }
}
