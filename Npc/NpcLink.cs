using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Npc
{
    public sealed class DependencyPropertyLink : ResourceContainer, ILink
    {
        private DependencyObject _source;
        private readonly DependencyProperty _declaration;
        private readonly DependencyPropertyDescriptor _descriptor;
        private readonly List<Action<object>> _changed = new List<Action<object>>();

        public DependencyPropertyLink(DependencyProperty declaration)
        {
            _declaration = declaration;
            _descriptor = DependencyPropertyDescriptor.FromProperty(declaration, declaration.OwnerType);
            Resources.Add(Unsubscribe);
        }

        public object Value { get; private set; }
        public Type FormalType => _declaration.PropertyType;

        public void Subscribe(Action<object> handler) => _changed.Add(handler);

        public void ChangeSource(object value)
        {
            if (ReferenceEquals(_source, value))
                return;
            if (_source != null)
            {
                _descriptor.AddValueChanged(_source, OnPropertyChanged);
            }
            _source = (DependencyObject)value;
            UpdateValue();
            if (_source != null)
            {
                _descriptor.RemoveValueChanged(_source, OnPropertyChanged);
            }
        }
        private void OnPropertyChanged(object sender, EventArgs eventArgs)
        {
            UpdateValue();
        }
        private void UpdateValue()
        {
            var value = _source.GetValue(_declaration);

            if (Equals(Value, value))
                return;
            Value = value;
            _changed.ForEach(handle => handle(value));
        }
        private void Unsubscribe()
        {
            if (_source != null)
                _descriptor.RemoveValueChanged(_source, OnPropertyChanged);
        }

        public override string ToString() => $"DependencyProperty({_declaration.Name})";
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