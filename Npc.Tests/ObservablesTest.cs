using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using static Npc.Tests.NpcSamples;

namespace Npc.Tests
{
    public sealed class ObservablesTest
    {
        private readonly S _z = new S("z", null);
        private readonly List<string> _log = new List<string>();
 
        [Fact]
        public void HelperMethod_Chain_Creates_S_Objects()
        {
            Chain(start: 'a', count: 5)[0].ToString().Should().Be("abcde");
        }
        [Fact]
        public void Subscription_Is_Marked_With_An_Asterisk()
        {
            var o1 = _z.Observe<string>(nameof(S.Name), _log.Add);
            _z.ToString().Should().Be("z*");
            var o2 = _z.Observe<string>(nameof(S.Name), _log.Add);
            _z.ToString().Should().Be("z**");
            o1.Dispose();
            _z.ToString().Should().Be("z*");
            o2.Dispose();
            _z.ToString().Should().Be("z");
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
            var original = Chain(start: 'a', count: 3);
            var replacement = Chain(start: 'd', count: 3);
            var observable = original[0].Track(s => s.X.X.Name);

            // Changing the property itself
            original[1].Name = "x";
            observable.Value.Should().Be("c");

            // Changing unrelated property upstream
            original[2].Name = "x";
            observable.Value.Should().Be("x");

            // Changing the chain in the middle
            original[1].X = replacement[2];
            observable.Value.Should().Be("f");

            // Changing the chain in the root
            original[0].X = replacement[0];
            original[0].ToString().Should().Be("a*d*e*f");
            observable.Value.Should().Be("e");
        }
        [Fact]
        public void Long_Chain()
        {
            var chain = Chain(start: 'a', count: 3);
            var observable = chain[0].Track(s => s.X.X.X);
            observable.Subscribe(s => _log.Add(s?.ToString() ?? "<null>"));

            chain[2].X = _z;
            observable.Value.Should().Be(_z);
            DrainLog().Should().Equal("z");
            chain[0].ToString().Should().Be("a*b*c*z");

            chain[1].X = null;
            chain[0].ToString().Should().Be("a*b*");
            observable.Value.Should().Be(null);
            DrainLog().Should().Equal("<null>");

            var replacement = Chain(start: 'd', count: 3);
            chain[0].X = replacement[0];
            observable.Value.ToString().Should().Be("f");
            DrainLog().Should().Equal("f");

            chain.Select(i => i.ToString()).Should().Equal("a*d*e*f", "b", "cz");
            replacement.Select(i => i.ToString()).Should().Equal("d*e*f", "e*f", "f");
            observable.Dispose();
            chain.Select(i => i.ToString()).Should().Equal("adef", "b", "cz");
            replacement.Select(i => i.ToString()).Should().Equal("def", "ef", "f");
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
        private List<string> DrainLog()
        {
            var result = _log.ToList();
            _log.Clear();
            return result;
        }
    }
}