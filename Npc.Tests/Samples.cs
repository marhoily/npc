using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Npc.Tests
{
    public static class Samples
    {
        public static S[] Chain(char start, int count)
        {
            var proto = Enumerable.Range(0, count)
                .Select(i => new S(new string((char)(start + i), 1), null))
                .ToArray();
            foreach (var p in proto.Zip(proto.Skip(1), (a, b) => new { a, b }))
                p.a.X = p.b;
            return proto;
        }
        public sealed class P
        {
            public P(S x) { X = x; }
            public S X { get; set; }
            public P Y => this;
        }
        public sealed class S : DependencyObject, INotifyPropertyChanged
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

            public ObservableCollection<int> Collection { get; } = new ObservableCollection<int>();
            public S(string name, S x)
            {
                _x = x;
                Name = name;
            }

            public static readonly DependencyProperty DProperty = DependencyProperty.Register(
                "D", typeof(S), typeof(S), new PropertyMetadata(default(S)));

            public S D
            {
                get { return (S) GetValue(DProperty); }
                set { SetValue(DProperty, value); }
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