using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Npc
{
    public sealed class SetObserver<T> : IDisposable
    {
        private ObservableCollection<T> _source;
        private readonly List<Action<SetChange<T>>> _changed = new List<Action<SetChange<T>>>();

        public HashSet<T> Value { get; } = new HashSet<T>();
        public void Subscribe(Action<SetChange<T>> handler) => _changed.Add(handler);
        public void ChangeSource(ObservableCollection<T> source)
        {
            if (ReferenceEquals(_source, source))
                return;
            if (_source != null)
            {
                _source.CollectionChanged -= Handler;
            }
            _source = source;
            UpdateValue();
            if (_source != null)
            {
                _source.CollectionChanged += Handler;
            }
        }

        private void UpdateValue()
        {
            var newSet = new HashSet<T>(_source);
            foreach (var item in Value)
                if (!newSet.Contains(item))
                {
                    Value.Remove(item);
                    _changed.ForEach(handle =>
                        handle(new SetChange<T>(SetOperation.Remove, item)));
                }
            foreach (var item in _source)
                if (Value.Add(item))
                {
                    _changed.ForEach(handle =>
                        handle(new SetChange<T>(SetOperation.Add, item)));
                }
        }

        private void Handler(object s, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                    {
                        if (!Value.Add(item)) throw new Exception("Duplicate!");
                        _changed.ForEach(handle =>
                            handle(new SetChange<T>(SetOperation.Add, item)));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        if (!Value.Remove(item)) throw new Exception("WTF!?");
                        _changed.ForEach(handle =>
                            handle(new SetChange<T>(SetOperation.Remove, item)));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            if (_source != null)
                _source.CollectionChanged -= Handler;
        }
    }
}