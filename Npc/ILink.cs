using System;

namespace Npc
{
    public interface ILink : IDisposable
    {
        object Value { get; }
        Type FormalType { get; }
        void Subscribe(Action<object> changed);
        void ChangeSource(object value);
    }
}