﻿using System;
using FluentAssertions;
using Xunit;

namespace Npc.Tests
{
    public sealed class SetObserverTest
    {
        private int _sum;
        private readonly Samples.S[] _original = Samples.Chain(start: 'a', count: 3);
        private readonly Samples.S[] _replacement = Samples.Chain(start: 'd', count: 3);

        [Fact]
        public void Track_Null()
        {
            var observer = _original[2].TrackSet(s => s.X.Collection);
            observer.Value.Should().BeEmpty();
            _replacement[0].Collection.Add(34);
            _original[2].X = _replacement[0];
            observer.Value.Should().Equal(34);
        }

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
        [Fact]
        public void When_Chain_Changes()
        {
            _original[2].Collection.Add(63);
            _replacement[2].Collection.Add(36);
            using (var setObserver = _original[0].TrackSet(s => s.X.X.Collection))
            {
                _original[0].X = _replacement[0].X;
                setObserver.Value.Should().Equal(36);
            }
        }
     
        [Fact]
        public void Should_Raise_Events()
        {
            using (var setObserver = _original[0].TrackSet(s => s.X.X.Collection))
            {
                setObserver.Added += n => _sum += n;
                setObserver.Removed += n => _sum -= n;
                _original[2].Collection.Add(1);
                _original[2].Collection.Add(2);
                _original[2].Collection.Add(4);
                _original[2].Collection.Remove(1);
                _sum.Should().Be(6);
                _replacement[2].Collection.Add(2);
                _replacement[2].Collection.Add(8);
                _original[0].X = _replacement[0].X;
                _sum.Should().Be(10);
            }
        }
    }
}