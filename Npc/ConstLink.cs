using System;
using System.Collections.Generic;

namespace Npc
{
    public sealed class ConstLink : ResourceContainer, ILink
    {
        private readonly string _name;
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
}