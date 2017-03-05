using System;
using System.Collections;
using JetBrains.Annotations;

namespace Npc
{
    public class SetSynchronizer<T> : IDisposable
    {
        private readonly IList _destination;
        public IObservableSet<T> Source { get; }

        public SetSynchronizer([NotNull]IObservableSet<T> source, [NotNull]IList destination)
        {
            _destination = destination;
            Source = source;
            Source.Added += OnAdded;
            Source.Removed += OnRemoved;
            foreach (var item in source.Value)
                _destination.Add(item);
        }

        private void OnRemoved(T obj)
        {
            _destination.Remove(obj);
        }

        private void OnAdded(T obj)
        {
            _destination.Add(obj);
        }

        public void Dispose()
        {
            Source.Added -= OnAdded;
            Source.Removed -= OnRemoved;
        }
    }
}