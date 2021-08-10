using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class BusOptionsDictionary<TOptions> : IBusOptionsDictionary<TOptions>
        where TOptions : class, new()
    {
        private readonly IDictionary<Enum, TOptions> _options;

        public TOptions this[Enum @switch]
        {
            get => _options[@switch];
            set => Add(@switch, value);
        }

        public BusOptionsDictionary()
        {
            _options = new ConcurrentDictionary<Enum, TOptions>();
        }

        public ICollection<Enum> Keys => _options.Keys;

        public ICollection<TOptions> Values => _options.Values;

        public int Count => _options.Count;

        public void Add(Enum key, TOptions value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (_options.ContainsKey(key))
                throw new ArgumentException($"Já existe uma configuração para o switch {key}", nameof(key));
            _options.Add(key, value);
        }

        public void Clear()
            => _options.Clear();

        public bool ContainsKey(Enum key)
            => _options.ContainsKey(key);

        public bool Remove(Enum key)
            => _options.Remove(key);

        public bool TryGetValue(Enum key, out TOptions value)
            => _options.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<Enum, TOptions>> GetEnumerator()
            => _options.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
