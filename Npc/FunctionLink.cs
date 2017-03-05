using System;
using System.Collections.Generic;

namespace Npc
{
    public sealed class ConstLink<T> : ResourceContainer, ILink
    {
        public object Value { get; }
        public Type FormalType => typeof(T);

        public ConstLink(object value)
        {
            Value = value;
        }

        public void Subscribe(Action<object, object> changed)
        {
        }

        public void ChangeSource(object value)
        {
            throw new NotSupportedException();
        }
    }
    public sealed class FunctionLink : ResourceContainer, ILink
    {
        private readonly string _name;
        private readonly Func<object, object> _exp;
        private readonly List<Action<object, object>> _changed = new List<Action<object, object>>();
        private object _source;
        public object Value { get; private set; }
        public void Subscribe(Action<object, object> handler) => _changed.Add(handler);
        public Type FormalType { get; }

        public FunctionLink(Type formalType, string name, Func<object, object> exp)
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
            var old = Value;
            Value = value;
            _changed.ForEach(handle => handle(old, value));
        }

        public override string ToString() => $"Function({_name})";
    }
}