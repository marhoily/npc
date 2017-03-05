using System.Linq;
using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class ExpressionsTest 
    {
        [Fact]
        public void Npc()
        {
            new S("x", null).GetLinks(s => s.Name)
                .Select(x => x.ToString()).Should().Equal("Npc(Name)");
        }
        [Fact]
        public void Const()
        {
            "x".GetLinks(s => s.Length)
                .Select(x => x.ToString()).Should().Equal("Const(Length)");
        }
        [Fact]
        public void Many()
        {
            new S("x", null).GetLinks(s => s.Name.Length)
                .Select(x => x.ToString()).Should().Equal("Npc(Name)", "Const(Length)");
        }
        [Fact]
        public void Should_Not_Group_Constants()
        {
            new P(new S("x", null)).GetLinks(s => s.Y.Y)
                .Select(x => x.ToString()).Should().Equal("Const(Y)", "Const(Y)");
        }
    }
}