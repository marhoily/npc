using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class DependencyPropertyLinkTest
    {
        private readonly S[] _replacement = Chain(start: 'd', count: 3);
        private readonly S[] _original = Chain(start: 'a', count: 3);
        [Fact]
        public void Should_Extract_Value()
        {
            _original[0].Track(s => s.D.Name).Value.Should().Be("b");
        }
        [Fact]
        public void Should_Follow_Change()
        {
            using (var valueObserver = _original[0].Track(s => s.D.Name))
            {
                _original[0].D = _replacement[0].X;
                valueObserver.Value.Should().Be("e");
            }
        }
        [Fact]
        public void Should_Unsubscribe()
        {
            using (_original[0].Track(s => s.X.D.Name))
            {
                _original[1].ToString().Should().Be("bc*");
                _original[0].X = _replacement[0].X;
                _original[1].ToString().Should().Be("bc");
            }
            _replacement[0].ToString().Should().Be("def");
        }
    }
}