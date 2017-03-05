using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Npc
{
    public sealed class SetSelector<TTo, TFrom> : IObservableSet<TTo>
    {
        private readonly IObservableSet<TFrom> _source;
        private readonly Func<TFrom, ValueObserver<TTo>> _selector;
        private readonly SelectorMap<TFrom, ValueObserver<TTo>, TTo> _map 
            = new SelectorMap<TFrom, ValueObserver<TTo>, TTo>(c => c.Value);

        public ICollection<TTo> Value => _map.CoreCollection;
        public event Action<TTo> Added;
        public event Action<TTo> Removed;
        public void Dispose() => _source.Dispose();

        public SetSelector(IObservableSet<TFrom> source, Func<TFrom, ValueObserver<TTo>> selector)
        {
            // It is important to track all the changes to both TFrom and TTo
            _source = source;
            _selector = selector;
            _source.Added += item => Added?.Invoke(Add(item));
            _source.Removed += item => Removed?.Invoke(Remove(item));
            foreach (var item in source.Value) Add(item);
        }

        private TTo Add(TFrom item)
        {
            return _map.Add(item, _selector(item)
                .WithSubscription((old, neu) =>
                {
                    Removed?.Invoke(old);
                    Added?.Invoke(neu);
                })).Value;
        }
        private TTo Remove(TFrom obj)
        {
            var observer = _map.Pop(obj);
            var result = observer.Value;
            observer.Dispose();
            return result;
        }

        public SetSelector<TTo, TFrom> With(Expression<Action<TFrom, TTo>> action)
        {
            // It is important to track all the changes to both TFrom and TTo
            throw new NotImplementedException();
            return this;
        }
    }
}