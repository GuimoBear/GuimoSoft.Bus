using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Kafka.Common;

namespace GuimoSoft.Bus.Tests.Fakes
{
    internal sealed class FakeClientBuilder : ClientBuilder
    {
        public FakeClientBuilder(IBusLogDispatcher logger) : base(logger)
        {
        }

        public void WriteLog()
            => LogMessage(ServerName.Default, Finality.Consume, new Confluent.Kafka.LogMessage("_d_", Confluent.Kafka.SyslogLevel.Info, "test", "test"));

        public void WriteException()
            => LogException(ServerName.Default, Finality.Consume, new Confluent.Kafka.Error(Confluent.Kafka.ErrorCode.BrokerNotAvailable, "test", true));
    }
}
