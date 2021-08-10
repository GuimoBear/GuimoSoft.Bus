using System;
using System.Runtime.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Exceptions
{
    [Serializable]
    public sealed class BusOptionsMissingException : Exception
    {
        public BusOptionsMissingException(BusName bus, Enum @switch, Type configType)
            : base(GetMessage(bus, @switch, configType)) { }

        private static string GetMessage(BusName bus, Enum @switch, Type configType)
        {
            if (ServerName.Default.Equals(@switch))
                return $"Está faltando o {configType.Name} para o {bus}";
            return $"Está faltando o {configType.Name} para o host '{@switch}' do {bus}";
        }

        private BusOptionsMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
