using System;
using System.Collections.Generic;

namespace Npc
{
    internal abstract class ResourceContainer : IDisposable
    {
        public List<Action> Resources { get; } = new List<Action>();
        public void Dispose() => Resources.ForEach(dispose => dispose());
    }
}
