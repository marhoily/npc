using System.Linq;
using FluentAssertions;
using Xunit;
using static Npc.Tests.Samples;

namespace Npc.Tests
{
    public sealed class VisitorTest 
    {
        [Fact]
        public void Npc()
        {
            new S("x", null).GetLinks(s => s.Name)
                .Select(x => x.ToString()).Should().Equal("Npc(Name)");
        }
        [Fact]
        public void Function()
        {
            "x".GetLinks(s => s.Length)
                .Select(x => x.ToString()).Should().Equal("Function(Length)");
        }
        [Fact]
        public void Many()
        {
            new S("x", null).GetLinks(s => s.Name.Length)
                .Select(x => x.ToString()).Should().Equal("Npc(Name)", "Function(Length)");
        }
        [Fact]
        public void Should_Not_Group_Functionants()
        {
            new P(new S("x", null)).GetLinks(s => s.Y.Y)
                .Select(x => x.ToString()).Should().Equal("Function(Y)", "Function(Y)");
        }
        [Fact]
        public void DependencyProperty()
        {
            new S("x", null).GetLinks(s => s.D)
                .Select(x => x.ToString()).Should().Equal("DependencyProperty(D)");
        }
    }
}