using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using static Npc.Tests.NpcSamples;

namespace Npc.Tests
{
    public sealed class ObservablesTest : INotifyPropertyChanged
    {
        private readonly List<string> _log = new List<string>();
        private readonly S[] _original = Chain(start: 'a', count: 3);
        private readonly S[] _replacement = Chain(start: 'd', count: 3);

        [Fact]
        public void HelperMethod_Chain_Creates_S_Objects()
        {
            Chain(start: 'a', count: 5)[0].ToString().Should().Be("abcde");
        }
        [Fact]
        public void Subscription_Is_Marked_With_An_Asterisk()
        {
            var o1 = _original[2]
                .Track<string>(nameof(S.Name))
                .WithSubscription(_log.Add);
            _original[2].ToString().Should().Be("c*");
            var o2 = _original[2]
                .Track<string>(nameof(S.Name))
                .WithSubscription(_log.Add);
            _original[2].ToString().Should().Be("c**");
            o1.Dispose();
            _original[2].ToString().Should().Be("c*");
            o2.Dispose();
            _original[2].ToString().Should().Be("c");
        }

        [Fact]
        public void Track_Should_Not_Accept_Paths_Of_Length_Zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                    () => Chain(start: 'a', count: 3)[0].Track(s => s))
                .Message.Should().Be(
                    "Track does not accept paths of length 0\r\n" +
                    "Parameter name: path");
        }
        [Fact]
        public void Track_Should_Observe_Correct_Value()
        {
            Chain(start: 'a', count: 3)[0].Track(s => s.Name).Value.Should().Be("a");
            Chain(start: 'a', count: 3)[0].Track(s => s.X.Name).Value.Should().Be("b");
            Chain(start: 'a', count: 3)[0].Track(s => s.X.X.Name).Value.Should().Be("c");
            Chain(start: 'a', count: 3)[0].Track(s => s.X.X.X.Name).Value.Should().BeNull();
            Chain(start: 'a', count: 3)[0].Track(s => s.X.X.X.X.Name).Value.Should().BeNull();

            Chain(start: 'a', count: 3)[0].Track(s => s.X).Value.ToString().Should().Be("bc");
            Chain(start: 'a', count: 3)[0].Track(s => s.X.X).Value.ToString().Should().Be("c");
            Chain(start: 'a', count: 3)[0].Track(s => s.X.X.X).Value.Should().BeNull();
        }
        [Fact]
        public void Should_Observe_Correct_Value_After_Changes()
        {
            var observable = _original[0].Track(s => s.X.X.Name);

            // Changing the property itself
            _original[1].Name = "x";
            observable.Value.Should().Be("c");

            // Changing unrelated property upstream
            _original[2].Name = "x";
            observable.Value.Should().Be("x");

            // Changing the chain in the middle
            _original[1].X = _replacement[2];
            observable.Value.Should().Be("f");

            // Changing the chain in the root
            _original[0].X = _replacement[0];
            _original[0].ToString().Should().Be("a*d*e*f");
            observable.Value.Should().Be("e");
        }
        [Fact]
        public void Should_Notify_Subscribers()
        {
            var observable = _original[0].Track(s => s.X.X.X);
            observable.Subscribe(s => _log.Add(s?.ToString() ?? "<null>"));

            _original[2].X = _replacement[0];
            DrainLog().Should().Equal("def");

            _original[1].X = null;
            DrainLog().Should().Equal("<null>");

            _original[0].X = _replacement[0];
            DrainLog().Should().Equal("f");
        }

        [Fact]
        public void Notifications_ShouldNotCome_When_MiddleOfTheChain_Changes_DoNot_Change_The_Result()
        {
            _original[1].X = _replacement[2];
            _original[0].ToString().Should().Be("abf");

            var observable = _original[0]
                .Track(s => s.X.X)
                .WithSubscription(s => _log.Add(s?.ToString() ?? "<null>"));
            observable.Value.Name.Should().Be("f");

            _original[0].X = _replacement[1];
            _original[0].ToString().Should().Be("a*e*f");

            DrainLog().Should().BeEmpty();
        }

        [Fact]
        public void Should_Unsubscribe_From_Replaced_Pieces_Of_The_Chain()
        {
            var rest = _original[1];
            var observable = _original[0].Track(s => s.X.X.Name);
            observable.Subscribe(s => { });
            rest.ToString().Should().Be("b*c*");
            _original[0].X = _replacement[1];
            rest.ToString().Should().Be("bc");
        }

        [Fact]
        public void Should_Work_For_Private_Properties_Too()
        {
            this.Track(x => x.Private.Name).Value.Should().Be("x");
        }

        [Fact]
        public void Should_Work_When_Some_Parts_Of_Path_Are_Not_Npc()
        {
            var observable = this.Track(x => x.Private.Name);
            observable.Value.Should().Be("x");
            Private.Name = "changed";
            observable.Value.Should().Be("changed");
            Private = _replacement[0];
            observable.Value.Should().Be("changed");
            OnPropertyChanged(nameof(Private));
            observable.Value.Should().Be("d");
        }
        private static S[] Chain(char start, int count)
        {
            var proto = Enumerable.Range(0, count)
                .Select(i => new S(new string((char)(start + i), 1), null))
                .ToArray();
            foreach (var p in proto.Zip(proto.Skip(1), (a, b) => new { a, b }))
                p.a.X = p.b;
            return proto;
        }
        private IEnumerable<string> DrainLog()
        {
            var result = _log.ToList();
            _log.Clear();
            return result;
        }

        private S Private { get; set; } = Chain('x', 3)[0];
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}