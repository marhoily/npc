using System;
using System.Collections.Generic;

namespace Npc
{
    public sealed class SelectorMap<TKey, TValue, TValueCore>
    {
        private readonly Func<TValue, TValueCore> _core;
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        public Dictionary<TKey, TValueCore> CollectionSource { get; } = new Dictionary<TKey, TValueCore>();
        public ICollection<TValueCore> CoreCollection => CollectionSource.Values;
        public ICollection<TKey> Keys => CollectionSource.Keys;

        public SelectorMap(Func<TValue, TValueCore> core)
        {
            _core = core;
        }

        public TValue Pop(TKey key)
        {
            var result = _map[key];
            _map.Remove(key);
            CollectionSource.Remove(key);
            return result;
        }
        public TValue Add(TKey key, TValue value)
        {
            _map.Add(key, value);
            CollectionSource.Add(key, _core(value));
            return value;
        }

        public TValue this[TKey key] => _map[key];
    }
}