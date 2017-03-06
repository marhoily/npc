using System;
using System.Collections.Generic;
using System.Linq;

namespace Npc
{
    public sealed class SetSelector<TTo, TFrom> : IObservableSet<TTo>
    {
        private readonly IObservableSet<TFrom> _source;
        private readonly Func<TFrom, ValueObserver<TTo>> _selector;
        private readonly SelectorMap<TFrom, ValueObserver<TTo>, TTo> _map
            = new SelectorMap<TFrom, ValueObserver<TTo>, TTo>(c => c.Value);

        public ICollection<TTo> Value => _map.CoreCollection;
        public Dictionary<TFrom, TTo> Map => _map.CollectionSource;
        public event Action<TTo> Added;
        public event Action<TTo> Removed;

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
            var observer = _map.Add(item, _selector(item)
                .WithSubscription((old, neu) =>
                {
                    Removed?.Invoke(old);
                    Added?.Invoke(neu);
                }));
            observer.Resources.AddRange(_extensions.Select(e => e(item, observer.Value)));
            return observer.Value;
        }
        private TTo Remove(TFrom obj)
        {
            var observer = _map.Pop(obj);
            var result = observer.Value;
            observer.Dispose();
            return result;
        }
        public void Dispose()
        {
            foreach (var item in _map.Keys.ToList())
                Remove(item);
            _source.Dispose();
        }
        private readonly List<Func<TFrom, TTo, Action>> _extensions = new List<Func<TFrom, TTo, Action>>();

        public SetSelector<TTo, TFrom> With<T>(Func<TFrom, ValueObserver<T>> track, Action<TTo, T> set)
        {
            Func<TFrom, TTo, Action> extension = (item, value) => track(item)
                .SubscribeAndApply((_, v) => set(value, v))
                .Dispose;
            _extensions.Add(extension);
            foreach (var item in _map.Keys.ToList())
            {
                var v = _map[item];
                v.Resources.Add(extension(item, v.Value));
            }
            return this;
        }
    }
}