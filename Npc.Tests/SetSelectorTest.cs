using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class UntrackedSetSelectorTest : IDisposable
    {
        private readonly S[] _original = Chain(start: 'a', count: 3);
        private readonly List<int> _destination = new List<int>();
        private readonly SetSynchronizer<int> _setSynchronizer;

        public UntrackedSetSelectorTest()
        {
            _original[0].Xs.Add(new S("123", null));
            _setSynchronizer = _original[0].TrackSet(s => s.Xs)
                .Select(s => s.Name.Length)
                .SynchronizeTo(_destination);
        }

        public void Dispose()
        {
            _setSynchronizer.Dispose();
            _setSynchronizer.Source.Dispose();
        }

        [Fact]
        public void AddItem()
        {
            _original[0].Xs.Add(new S("12", null));
            _destination.Should().Equal(3,2);
        }
        [Fact]
        public void RemoveItem()
        {
            _original[0].Xs.RemoveAt(0);
            _destination.Should().BeEmpty();
        }
        [Fact]
        public void Change_Untracked_Item()
        {
            _original[0].Xs[0].Name = "1234";
            _destination.Should().Equal(3);
        }
    }
    public sealed class TrackedSetSelectorTest : IDisposable
    {
        private readonly S[] _original = Chain(start: 'a', count: 3);
        private readonly List<int> _destination = new List<int>();
        private readonly SetSynchronizer<int> _setSynchronizer;

        public TrackedSetSelectorTest()
        {
            _original[0].Xs.Add(new S("123", null));
            _setSynchronizer = _original[0].TrackSet(s => s.Xs)
                .Select(s => s.Track(x => x.Name.Length))
                .SynchronizeTo(_destination);
        }

        public void Dispose()
        {
            _setSynchronizer.Dispose();
            _setSynchronizer.Source.Dispose();
        }

        [Fact]
        public void AddItem()
        {
            _original[0].Xs.Add(new S("12", null));
            _destination.Should().Equal(3,2);
        }
        [Fact]
        public void RemoveItem()
        {
            _original[0].Xs.RemoveAt(0);
            _destination.Should().BeEmpty();
        }
        [Fact]
        public void Change_Untracked_Item()
        {
            _original[0].Xs[0].Name = "1234";
            _destination.Should().Equal(4);
        }
    }
}
