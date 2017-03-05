using System;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Reflection.BindingFlags;

namespace Npc
{

    public interface ILink : IDisposable
    {
        object Value { get; }
        Type FormalType { get; }
        void Subscribe(Action<object> changed);
        void ChangeSource(object value);
    }

    public sealed class ConstLink : ResourceContainer, ILink
    {
        private string _name;
        private readonly Func<object, object> _exp;
        private readonly List<Action<object>> _changed = new List<Action<object>>();
        private object _source;
        public object Value { get; private set; }
        public void Subscribe(Action<object> handler) => _changed.Add(handler);
        public Type FormalType { get; }

        public ConstLink(Type formalType, string name, Func<object, object> exp)
        {
            _name = name;
            _exp = exp;
            FormalType = formalType;
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

        public override string ToString() => $"Const({_name})";
    }
    public sealed class NpcLink : ResourceContainer, ILink
    {
        private INotifyPropertyChanged _source;
        private readonly string _propertyName;
        private readonly List<Action<object>> _changed = new List<Action<object>>();
        public Type FormalType { get; }
        public override string ToString() => $"Npc({_propertyName})";

        public NpcLink(Type formalType, string propertyName)
        {
            _propertyName = propertyName;
            FormalType = formalType;
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
            if (e.PropertyName == _propertyName)
                UpdateValue();
        }
        private void UpdateValue()
        {
            var value = _source?
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
}