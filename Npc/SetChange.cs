namespace Npc
{
    public struct SetChange<T>
    {
        public SetOperation Operation { get; }
        public T Value { get; }

        public SetChange(SetOperation operation, T value)
        {
            Operation = operation;
            Value = value;
        }
    }
}