using Confluent.Kafka;
using FluentAssertions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core
{
    public class ExceptionTests
    {
        private void SerializeTest<TException>(TException ex) where TException : Exception
        {
            using var mem = new MemoryStream();
            var bf = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            bf.Serialize(mem, ex);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            mem.Position = 0;

#pragma warning disable SYSLIB0011 // Type or member is obsolete
            var newEx = bf.Deserialize(mem);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            newEx
                .Should().BeEquivalentTo(ex);
        }

        [Fact]
        public void Se_ConstruirBusAlreadyConfiguredException_Entao_NaoEstouraErro()
        {
            _ = new BusAlreadyConfiguredException(BusName.Kafka, ServerName.Default);
            SerializeTest(new BusAlreadyConfiguredException(BusName.Kafka, ServerName.Default));
            SerializeTest(new BusAlreadyConfiguredException(BusName.Kafka, FakeServerName.FakeHost1));
        }

        [Fact]
        public void Se_ConstruirBusOptionsMissingException_Entao_NaoEstouraErro()
        {
            _ = new BusOptionsMissingException(BusName.Kafka, ServerName.Default, typeof(ProducerConfig));
            SerializeTest(new BusOptionsMissingException(BusName.Kafka, ServerName.Default, typeof(ProducerConfig)));
            SerializeTest(new BusOptionsMissingException(BusName.Kafka, FakeServerName.FakeHost1, typeof(ProducerConfig)));
        }
    }
}
