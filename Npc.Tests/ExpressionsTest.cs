using System.Linq;
using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class ExpressionsTest 
    {
        [Fact]
        public void NpcLink()
        {
            new S("x", null).GetLinks(s => s.Name)
                .Select(x => x.ToString()).Should().Equal("NpcLink(Name)");
        }
        [Fact]
        public void ConstLink()
        {
            "x".GetLinks(s => s.Length)
                .Select(x => x.ToString()).Should().Equal("ConstLink(Length)");
        }
        [Fact]
        public void Many()
        {
            new S("x", null).GetLinks(s => s.Name.Length)
                .Select(x => x.ToString()).Should().Equal("NpcLink(Name)", "ConstLink(Length)");
        }
    }
}