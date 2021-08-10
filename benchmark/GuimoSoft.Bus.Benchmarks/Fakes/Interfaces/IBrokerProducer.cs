namespace GuimoSoft.Bus.Benchmarks.Fakes.Interfaces
{
    public interface IBrokerProducer
    {
        void Enqueue(string topic, byte[] message);
    }
}
