using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GuimoSoft.Bus.Abstractions
{
    public sealed class ConsumeInformations
    {
        private readonly IDictionary<string, string> _headers;

        public BusName Bus { get; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Enum? Switch { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public string Endpoint { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }

        internal ConsumeInformations(BusName bus, Enum @switch, string endpoint)
        {
            Bus = bus;
            Switch = @switch.Equals(ServerName.Default) ? null : @switch;
            Endpoint = endpoint;
            _headers = new Dictionary<string, string>();
            Headers = new ReadOnlyDictionary<string, string>(_headers);
        }

        internal void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("É necessario informar uma chave não vazia no header", nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("É necessario informar um valor não vazio no header", nameof(value));
            _headers[key] = value;
        }
    }
}
