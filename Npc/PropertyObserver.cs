using System;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Reflection.BindingFlags;

namespace Npc
{
    internal sealed class PropertyObserver<T> : ResourceContainer, IObservable<T>
    {
        private INotifyPropertyChanged _source;
        private readonly string _propertyName;
        private readonly List<Action<T>> _changed = new List<Action<T>>();

        public PropertyObserver(string propertyName)
        {
            _propertyName = propertyName;
            Resources.Add(Unsubscribe);
        }

        public T Value { get; private set; }
        public void Subscribe(Action<T> handler) => _changed.Add(handler);
        public void ChangeSource(INotifyPropertyChanged source)
        {
            if (ReferenceEquals(_source, source))
                return;
            if (_source != null)
            {
                _source.PropertyChanged -= OnPropertyChanged;
            }
            _source = source;
            UpdateValue();
            if (_source != null)
            {
                _source.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _propertyName)
                UpdateValue();
        }
        private void UpdateValue()
        {
            var value = (T)_source?
                .GetType()
                .GetProperty(_propertyName, Instance | Public | NonPublic)
                .GetValue(_source);

            if (Equals(Value, value))
                return;
            Value = value;
            _changed.ForEach(handle => handle(value));
        }
        private void Unsubscribe()
        {
            if (_source != null)
                _source.PropertyChanged -= OnPropertyChanged;
        }
    }
    public interface ILink
    {
        object Value { get; }
        void Subscribe(Action<object> changed);
        void ChangeSource(object value);
    }

    public sealed class ConstLink : ResourceContainer, ILink
    {
        public string Name { get; }
        private readonly Func<object, object> _exp;
        private readonly List<Action<object>> _changed = new List<Action<object>>();
        private object _source;
        public object Value { get; private set; }
        public void Subscribe(Action<object> handler) => _changed.Add(handler);

        public ConstLink(string name, Func<object, object> exp)
        {
            Name = name;
            _exp = exp;
        }

        public void ChangeSource(object source)
        {
            if (Equals(_source, source)) return;
            _source = source;
            var value = _exp(_source);
            if (Equals(value, Value)) return;
            Value = value;
            _changed.ForEach(handle => handle(value));
        }
    }
    public sealed class NpcLink : ResourceContainer, ILink
    {
        private INotifyPropertyChanged _source;
        public readonly string PropertyName;
        private readonly List<Action<object>> _changed = new List<Action<object>>();

        public NpcLink(string propertyName)
        {
            PropertyName = propertyName;
            Resources.Add(Unsubscribe);
        }

        public object Value { get; private set; }
        public void Subscribe(Action<object> handler) => _changed.Add(handler);
        public void ChangeSource(object source)
        {
            if (ReferenceEquals(_source, source))
                return;
            if (_source != null)
            {
                _source.PropertyChanged -= OnPropertyChanged;
            }
            _source = (INotifyPropertyChanged) source;
            UpdateValue();
            if (_source != null)
            {
                _source.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PropertyName)
                UpdateValue();
        }
        private void UpdateValue()
        {
            var value = _source?
                .GetType()
                .GetProperty(PropertyName, Instance | Public | NonPublic)
                .GetValue(_source);

            if (Equals(Value, value))
                return;
            Value = value;
            _changed.ForEach(handle => handle(value));
        }
        private void Unsubscribe()
        {
            if (_source != null)
                _source.PropertyChanged -= OnPropertyChanged;
        }
    }
}