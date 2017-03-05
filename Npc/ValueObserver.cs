using System;
using System.ComponentModel;

namespace Npc
{
    public sealed class ValueObserver<T> : ResourceContainer, INotifyPropertyChanged
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
            Resources.Add(_link.Dispose);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ValueObserver<T> WithSubscription(Action<T, T> handler)
        {
            _link.Subscribe((a,b) => handler((T)a, (T)b));
            return this;
        }
        public ValueObserver<T> SubscribeAndApply(Action<T, T> handler)
        {
            _link.Subscribe((a,b) => handler((T)a, (T)b));
            handler(default(T), (T) _link.Value);
            return this;
        }
    }
}