using System;
using System.Runtime.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Exceptions
{
    [Serializable]
    public sealed class BusAlreadyConfiguredException : Exception
    {
        public BusAlreadyConfiguredException(BusName bus, Enum @switch)
            : base(GetMessage(bus, @switch)) { }

        private static string GetMessage(BusName bus, Enum @switch)
        {
            if (ServerName.Default.Equals(@switch))
                return $"O {bus} já foi configurado";
            return $"O host '{@switch}' do {bus} já foi configurado";
        }

        private BusAlreadyConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
