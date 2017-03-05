using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Npc.Tests
{
    public static class NpcSamples
    {
        public sealed class P
        {
            public P(S x) { X = x; }
            public S X { get; set; }
        }
        public sealed class S : INotifyPropertyChanged
        {
            public string Name
            {
                get { return _name; }
                set
                {
                    if (Equals(value, _name)) return;
                    _name = value;
                    OnPropertyChanged();
                }
            }

            public P Y => new P(X);
            private S _x;
            public S X
            {
                get { return _x; }
                set
                {
                    if (Equals(value, _x)) return;
                    _x = value;
                    OnPropertyChanged();
                }
            }

            public S(string name, S x)
            {
                _x = x;
                Name = name;
            }

            public override string ToString()
            {
                return Name + new string(c: '*', count: _handlersList.Count) + X;
            }

            private readonly List<PropertyChangedEventHandler> _handlersList = new List<PropertyChangedEventHandler>();
            private string _name;

            public event PropertyChangedEventHandler PropertyChanged
            {
                add { _handlersList.Add(value); }
                remove { _handlersList.Remove(value); }
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                foreach (var handler in _handlersList)
                    handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}