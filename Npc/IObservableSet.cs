using System;
using System.Collections.Generic;

namespace Npc
{
    public interface IObservableSet<T> : IDisposable
    {
        event Action<T> Added;
        event Action<T> Removed;
        ICollection<T> Value { get; }
    }
}