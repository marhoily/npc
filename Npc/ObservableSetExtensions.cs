using System;
using System.Collections;

namespace Npc
{
    public static class ObservableSetExtensions
    {
        public static SetSelector<TTo, TFrom> Select<TTo, TFrom>(
            this IObservableSet<TFrom> source, Func<TFrom, TTo> selector)
        {
            return new SetSelector<TTo,TFrom>(source, selector);
        }

        public static SetSynchronizer<T> SynchronizeTo<T>(
            this IObservableSet<T> source, IList destination)
        {
            return new SetSynchronizer<T>(source, destination);
        }
    }
}