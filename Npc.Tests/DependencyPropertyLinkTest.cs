using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class DependencyPropertyLinkTest
    {
        private readonly S[] _original = Chain(start: 'a', count: 3);
        [Fact]
        public void Should_Extract_Value()
        {
            _original[0].Track(x => x.D.Name).Value.Should().Be("b");
        }
    }
}