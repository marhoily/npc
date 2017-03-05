using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Npc
{
    public static class NpcExtensions
    {
        public static Action Track<T>(this INotifyPropertyChanged trackable, string propertyName, Action<T> onChanged)
        {
            if (trackable == null) return () => { };
            PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == propertyName) onChanged((T)s);
            };
            trackable.PropertyChanged += handler;
            return () => trackable.PropertyChanged -= handler;
        }

        public static IObservable<TResult> Track<TResult>(this INotifyPropertyChanged source, Expression<Func<TResult>> pathExpression)
        {
            return source.Track<TResult>(pathExpression.ExtractPropertyNames().Skip(count: 1).ToArray());
        }
        public static IObservable<TResult> Track<TSource, TResult>(this TSource source, Expression<Func<TSource, TResult>> pathExpression)
            where TSource : INotifyPropertyChanged
        {
            return source.Track<TResult>(pathExpression.ExtractPropertyNames().Skip(count: 1).ToArray());
        }

        private static IObservable<T> Track<T>(this INotifyPropertyChanged source, params string[] path)
        {
            if (path.Length == 0) throw new ArgumentOutOfRangeException(nameof(path), "Track does not accept paths of length 0");
            if (path.Length == 1) return source.Observe<T>(path.Single());
            var first = source.Observe<INotifyPropertyChanged>(path.First());
            var middle = path.Skip(count: 1).Take(path.Length - 2);
            return middle.Aggregate(first, (current, part)
                    => current.Observe<INotifyPropertyChanged>(part))
                .Observe<T>(path.Last());
        }
        public static IObservable<T> Observe<T>(this INotifyPropertyChanged source, string propertyName, Action<T> handler = null)
        {
            return CreateNpc(source, propertyName, handler);
        }
        public static IObservable<T> Observe<T>(this IObservable<INotifyPropertyChanged> source, string propertyName, Action<T> handler = null)
        {
            var npc = CreateNpc(source.Value, propertyName, handler);
            npc.Resources.Add(source.Dispose);
            source.Subscribe(npc.ChangeSource);
            return npc;
        }

        private static Npc<T> CreateNpc<T>(INotifyPropertyChanged source, string propertyName, Action<T> handler)
        {
            var npc = new Npc<T>(propertyName);
            npc.ChangeSource(source);
            if (handler != null) npc.Subscribe(handler);
            return npc;
        }
       

        public static CollectionObserver<T> Track<T>(
            this IObservable<ObservableCollection<T>> collectionSource,
            Action<T> added, Action<T> removed)
        {
            return new CollectionObserver<T>(collectionSource, added, removed);
        }
    }
}