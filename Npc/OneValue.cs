using System;
using System.ComponentModel;

namespace Npc
{
    public sealed class OneValue<T> : IDisposable, INotifyPropertyChanged
    {
        private readonly ILink _link;
        public T Value => (T)_link.Value;

        public OneValue(ILink link)
        {
            if (!typeof(T).IsAssignableFrom(link.FormalType))
                throw new Exception();
            _link = link;
            _link.Subscribe(_ => PropertyChanged?
                .Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
        }

        public void Dispose() => _link.Dispose();
        public event PropertyChangedEventHandler PropertyChanged;
        public OneValue<T> WithSubscription(Action<T> handler)
        {
            _link.Subscribe(x => handler((T)x));
            return this;
        }
    }
}