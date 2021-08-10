using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IMessageTypeCache
    {
        void Add(BusName busName, Finality finality, Enum @switch, Type type, string endpoint);

        IEnumerable<Enum> GetSwitchers(BusName busName, Finality finality);
        IEnumerable<string> GetEndpoints(BusName busName, Finality finality, Enum @switch);

        IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, IEvent message);

        IEnumerable<string> Get(BusName busName, Finality finality, Enum @switch, Type messageType);

        IReadOnlyCollection<Type> Get(BusName busName, Finality finality, Enum @switch, string endpoint);

        IEnumerable<(BusName BusName, Enum Switch, string Endpoint)> Get(Type messageType);
    }
}
