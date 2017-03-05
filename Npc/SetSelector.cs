using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Npc
{
    public sealed class SetSelector<TTo, TFrom> : IObservableSet<TTo>
    {
        private readonly IObservableSet<TFrom> _source;
        private readonly Func<TFrom, TTo> _selector;
        private readonly Dictionary<TFrom, TTo> _map = new Dictionary<TFrom, TTo>();

        public ICollection<TTo> Value => _map.Values;
        public event Action<TTo> Added;
        public event Action<TTo> Removed;
        public void Dispose() => _source.Dispose();

        public SetSelector(IObservableSet<TFrom> source, Func<TFrom, TTo> selector)
        {
            // It is important to track all the changes to both TFrom and TTo
            _source = source;
            _selector = selector;
            _source.Added += OnAdded;
            _source.Removed += OnRemoved;
            foreach (var item in source.Value)
                _map.Add(item, _selector(item));
        }

        public SetSelector<TTo, TFrom> With(Expression<Action<TFrom, TTo>> action)
        {
            // It is important to track all the changes to both TFrom and TTo
            throw new NotImplementedException();
            return this;
        }
        private void OnRemoved(TFrom obj)
        {
            var result = _map[obj];
            _map.Remove(obj);
            Removed?.Invoke(result);
        }
        private void OnAdded(TFrom obj)
        {
            var convert = _selector(obj);
            _map.Add(obj, convert);
            Added?.Invoke(convert);
        }
    }
}