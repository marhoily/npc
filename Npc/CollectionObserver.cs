using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Npc
{
    public sealed class CollectionObserver<T> : IDisposable
    {
        private readonly IObservable<ObservableCollection<T>> _collectionSource;
        private ObservableCollection<T> _collection;
        private readonly Action<T> _added;
        private readonly Action<T> _removed;

        public CollectionObserver(
            IObservable<ObservableCollection<T>> collectionSource,
            Action<T> added, Action<T> removed)
        {
            _collectionSource = collectionSource;
            _added = added;
            _removed = removed;
            UpdateCollection(collectionSource.Value);
            collectionSource.Subscribe(UpdateCollection);
        }

        private void UpdateCollection(ObservableCollection<T> collection)
        {
            if (_collection != null)
            {
                _collection.CollectionChanged -= Handler;
                foreach (var item in _collection)
                    _removed(item);
            }
            _collection = collection;
            if (_collection != null)
            {
                _collection.CollectionChanged += Handler;
                foreach (var item in _collection)
                    _added(item);
            }
        }
        private void Handler(object s, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                        _added(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                        _removed(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            _collectionSource.Dispose();
            if (_collection != null)
                _collection.CollectionChanged -= Handler;
        }
    }
}