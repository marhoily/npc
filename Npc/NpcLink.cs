using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Npc
{
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
        public void ChangeSource(object value)
        {
            if (ReferenceEquals(_source, value))
                return;
            if (_source != null)
            {
                _source.PropertyChanged -= OnPropertyChanged;
            }
            _source = (INotifyPropertyChanged) value;
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
                .GetProperty(_propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
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