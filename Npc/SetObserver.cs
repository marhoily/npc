using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using static System.Collections.Specialized.NotifyCollectionChangedAction;

namespace Npc
{
    public sealed class SetObserver<T> : IObservableSet<T>
    {
        [NotNull]
        private readonly ILink _link;
        private ObservableCollection<T> _source;
        public event Action<T> Added;
        public event Action<T> Removed;

        public HashSet<T> Value { get; } = new HashSet<T>();
        ICollection<T> IObservableSet<T>.Value => Value;

        public SetObserver([NotNull] ILink link)
        {
            _link = link;
            _link.Subscribe(value => OnSourceChanged((ObservableCollection<T>)value));
            OnSourceChanged((ObservableCollection<T>)link.Value);
        }

        private void OnSourceChanged(ObservableCollection<T> source)
        {
            if (_source != null)
            {
                _source.CollectionChanged -= OnCollectionChanged;
            }
            _source = source;
            OnValueChanged();
            if (_source != null)
            {
                _source.CollectionChanged += OnCollectionChanged;
            }
        }
        private void OnValueChanged()
        {
            var newSet = new HashSet<T>(_source);
            foreach (var item in Value.ToList())
                if (!newSet.Contains(item))
                {
                    Value.Remove(item);
                    Removed?.Invoke(item);
                }
            foreach (var item in _source)
                if (Value.Add(item))
                {
                    Added?.Invoke(item);
                }
        }
        private void OnCollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case Add:
                    foreach (T item in e.NewItems)
                    {
                        if (!Value.Add(item))
                            throw new InvalidOperationException(
                                $"The ObservableCollection<{typeof(T).Name}> is being tracked as a Set. " +
                                "Sets do not allow duplicate values. " +
                                $"A value {item} is already present in the collection");
                        Added?.Invoke(item);
                    }
                    break;
                case Remove:
                    foreach (T item in e.OldItems)
                    {
                        if (!Value.Remove(item)) throw new Exception("WTF!?");
                        Removed?.Invoke(item);
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        $"The ObservableCollection<{typeof(T).Name}> is being tracked as a Set. " +
                        $"{e.Action} operation is not supported!");
            }
        }

        public void Dispose()
        {
            _link.Dispose();
            if (_source != null)
                _source.CollectionChanged -= OnCollectionChanged;
        }
    }
}