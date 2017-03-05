using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Npc
{
    public static class ObservableExtensions
    {
        public static IObservable<TResult> Track<TSource, TResult>(this TSource source, Expression<Func<TSource, TResult>> pathExpression)
            where TSource : INotifyPropertyChanged
        {
            return source.Track<TResult>(pathExpression.ExtractPropertyNames().Skip(count: 1).ToArray());
        }
        public static IObservable<T> Track<T>(this INotifyPropertyChanged source, params string[] path)
        {
            if (path.Length == 0) throw new ArgumentOutOfRangeException(nameof(path), "Track does not accept paths of length 0");
            if (path.Length == 1) return source.ObserveProperty<T>(path.Single());
            var first = (IObservable<INotifyPropertyChanged>)
                source.ObserveProperty<INotifyPropertyChanged>(path.First());
            var middle = path.Skip(count: 1).Take(path.Length - 2);
            return middle.Aggregate(first, (current, part)
                    => current.ChainObservable<INotifyPropertyChanged>(part))
                    .ChainObservable<T>(path.Last());
        }
        public static IObservable<T> WithSubscription<T>(this IObservable<T> src, Action<T> handler)
        {
            src.Subscribe(handler);
            return src;
        }

        private static PropertyObserver<T> ObserveProperty<T>(this INotifyPropertyChanged source, string propertyName)
        {
            var npc = new PropertyObserver<T>(propertyName);
            npc.ChangeSource(source);
            return npc;
        }
        private static IObservable<T> ChainObservable<T>(this IObservable<INotifyPropertyChanged> source, string propertyName)
        {
            var npc = ObserveProperty<T>(source.Value, propertyName);
            npc.Resources.Add(source.Dispose);
            source.Subscribe(npc.ChangeSource);
            return npc;
        }
    }
}