using System;
using System.Collections;

namespace Npc
{
    public static class ObservableSetExtensions
    {
        public static SetSelector<TTo, TFrom> Select<TTo, TFrom>(
            this IObservableSet<TFrom> source, Func<TFrom, TTo> selector)
        {
            return new SetSelector<TTo, TFrom>(source, x =>
                new ValueObserver<TTo>(new ConstLink<TTo>(selector(x))));
        }
        public static SetSelector<TTo, TFrom> Select<TTo, TFrom>(
            this IObservableSet<TFrom> source, Func<TFrom, ValueObserver<TTo>> selector)
        {
            return new SetSelector<TTo,TFrom>(source, selector);
        }

        public static SetSynchronizer<T> SynchronizeTo<T>(
            this IObservableSet<T> source, IList destination)
        {
            return new SetSynchronizer<T>(source, destination);
        }
        public static void DisposeWithSource<T>(this SetSynchronizer<T> source)
        {
            source.Dispose();
            source.Source.Dispose();
        }
    }
}