using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IBusOptionsDictionary<TOptions> : IEnumerable<KeyValuePair<Enum, TOptions>>
        where TOptions : class, new()
    {
        TOptions this[Enum @switch] { get; set; }

        int Count { get; }
        ICollection<Enum> Keys { get; }
        ICollection<TOptions> Values { get; }

        void Add(Enum key, TOptions value);
        void Clear();
        bool ContainsKey(Enum key);
        bool Remove(Enum key);
        bool TryGetValue(Enum key, out TOptions value);
    }
}
