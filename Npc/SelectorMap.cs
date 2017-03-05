using System;
using System.Collections.Generic;

namespace Npc
{
    public sealed class SelectorMap<TKey, TValue, TValueCore>
    {
        private readonly Func<TValue, TValueCore> _core;
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, TValueCore> _collectionSource = new Dictionary<TKey, TValueCore>();
        public ICollection<TValueCore> CoreCollection => _collectionSource.Values;

        public SelectorMap(Func<TValue, TValueCore> core)
        {
            _core = core;
        }

        public TValue Pop(TKey key)
        {
            var result = _map[key];
            _map.Remove(key);
            _collectionSource.Remove(key);
            return result;
        }
        public TValue Add(TKey key, TValue value)
        {
            _map.Add(key, value);
            _collectionSource.Add(key, _core(value));
            return value;
        }
    }
}