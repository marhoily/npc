using System;
using System.ComponentModel;

namespace Npc
{
    public sealed class ValueObserver<T> : IDisposable, INotifyPropertyChanged
    {
        private readonly ILink _link;
        public T Value => (T)_link.Value;

        public ValueObserver(ILink link)
        {
            if (!typeof(T).IsAssignableFrom(link.FormalType))
                throw new Exception();
            _link = link;
            _link.Subscribe((a,b) => PropertyChanged?
                .Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
        }

        public void Dispose() => _link.Dispose();
        public event PropertyChangedEventHandler PropertyChanged;
        public ValueObserver<T> WithSubscription(Action<T, T> handler)
        {
            _link.Subscribe((a,b) => handler((T)a, (T)b));
            return this;
        }
    }
}