using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class SetObserverTest 
    {
       // private readonly List<string> _log = new List<string>();
        private readonly S[] _original = Chain(start: 'a', count: 3);
        private readonly S[] _replacement = Chain(start: 'd', count: 3);
        
        [Fact]
        public void Track_Should_Observe_Correct_Value()
        {
            _original[2].Collection.Add(63);
            _original[0].TrackSet(s => s.X.X.Collection).Value.Should().Equal(63);
        }
        
        [Fact]
        public void Track_Should_Observe_Correct_Value_After_The_Fact()
        {
            _original[2].Collection.Add(63);
            using (var setObserver = _original[0].TrackSet(s => s.X.X.Collection))
            {
                _original[2].Collection.Remove(63);
                _original[2].Collection.Add(36);
                setObserver.Value.Should().Equal(36);
            }
        }
        [Fact]
        public void TrackSet_Should_Not_Allow_Duplicate_Values()
        {
            _original[2].Collection.Add(63);
            _original[0].TrackSet(s => s.X.X.Collection);
            Assert.Throws<InvalidOperationException>(() => _original[2].Collection.Add(63))
                .Message.Should().Be(
                    "The ObservableCollection<Int32> is being tracked as a Set. " +
                    "Sets do not allow duplicate values. " +
                    "A value 63 is already present in the collection");
        }
        [Fact]
        public void Operation_Is_Not_Supported()
        {
            _original[2].Collection.Add(63);
            _original[0].TrackSet(s => s.X.X.Collection);
            Assert.Throws<InvalidOperationException>(() => _original[2].Collection[0] = 36)
                .Message.Should().Be(
                    "The ObservableCollection<Int32> is being tracked as a Set. " +
                    "Replace operation is not supported!");
        }
    }
}