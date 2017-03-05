using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Npc
{
    public sealed class DependencyPropertyLink : ResourceContainer, ILink
    {
        private DependencyObject _source;
        private readonly DependencyProperty _declaration;
        private readonly DependencyPropertyDescriptor _descriptor;
        private readonly List<Action<object, object>> _changed = new List<Action<object, object>>();

        public DependencyPropertyLink(DependencyProperty declaration)
        {
            _declaration = declaration;
            _descriptor = DependencyPropertyDescriptor.FromProperty(declaration, declaration.OwnerType);
            Resources.Add(Unsubscribe);
        }

        public object Value { get; private set; }
        public Type FormalType => _declaration.PropertyType;

        public void Subscribe(Action<object, object> handler) => _changed.Add(handler);

        public void ChangeSource(object value)
        {
            if (ReferenceEquals(_source, value))
                return;
            if (_source != null)
            {
                _descriptor.RemoveValueChanged(_source, OnPropertyChanged);
            }
            _source = (DependencyObject)value;
            UpdateValue();
            if (_source != null)
            {
                _descriptor.AddValueChanged(_source, OnPropertyChanged);
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
            var old = Value;
            Value = value;
            _changed.ForEach(handle => handle(old, value));
        }
        private void Unsubscribe()
        {
            if (_source != null)
                _descriptor.RemoveValueChanged(_source, OnPropertyChanged);
        }

        public override string ToString() => $"DependencyProperty({_declaration.Name})";
    }
}