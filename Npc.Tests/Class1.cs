using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Xunit;

namespace Npc.Tests
{
    public sealed class MyVisitorTest
    {
        sealed class S
        {
            public S(int depth) { Depth = depth; }
            public S X => Depth == 0 ? null : new S(Depth - 1);
            public int Depth { get; }
        }

        private static List<Expression<Func<object>>> Check(Expression<Func<S>> exp)
        {
            return exp.ExtractSubexpressions().ToList();
        }

        [Fact]
        public void CheckDepths()
        {
            new S(3).X.X.X.Depth.Should().Be(0);
            new S(3).X.X.Depth.Should().Be(1);
            new S(3).X.Depth.Should().Be(2);
            new S(3).Depth.Should().Be(3);
        }
        [Fact]
        public void CheckToString()
        {
            Check(() => new S(3).X.X.X).Select(e => e.Body.ToString())
                .Should().Equal("new S(3)", "new S(3).X", "new S(3).X.X", "new S(3).X.X.X");
        }
        [Fact]
        public void Compile()
        {
            var expressions = Check(() => new S(3).X.X.X);
            expressions[3].Compile()().As<S>().Depth.Should().Be(0);
            expressions[2].Compile()().As<S>().Depth.Should().Be(1);
            expressions[1].Compile()().As<S>().Depth.Should().Be(2);
            expressions[0].Compile()().As<S>().Depth.Should().Be(3);
        }
    }
}