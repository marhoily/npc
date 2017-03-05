using System;

namespace Npc
{
    public interface IObservable<out T> : IDisposable
    {
        T Value { get; }
        void Subscribe(Action<T> handler);
    }

    public static class ObservableExtensions
    {
        public static IObservable<T> WithSubscription<T>(this IObservable<T> src, Action<T> handler)
        {
            src.Subscribe(handler);
            return src;
        }
    }
}