using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Xunit;
using static System.Reflection.BindingFlags;
using static Npc.Tests.Samples;

namespace Npc.Tests
{

    public sealed class ExpressionsTest 
    {
        private static List<ILink> Exp<T>(Expression<Func<T>> exp)
        {
            var visitor = new Visitor();
            visitor.Visit(exp.Body);
            return visitor.Chain;
        }

        [Fact]
        public void NpcLink()
        {
            var links = Exp(() => new S("x", null).X);
            links.Single().Should().BeOfType<NpcLink>()
                .Which.PropertyName.Should().Be("X");
        }
        [Fact]
        public void ConstLink()
        {
            var links = Exp(() => new S("x", null).Y);
            links.Single().Should().BeOfType<ConstLink>()
                .Which.Name.Should().Be("Y");
        }

        sealed class Visitor : ExpressionVisitor
        {
            public List<ILink> Chain { get; } = new List<ILink>();
            protected override Expression VisitMember(MemberExpression node)
            {
                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(propertyInfo.PropertyType))
                        Chain.Add(new NpcLink(propertyInfo.Name));
                    else
                        Chain.Add(new ConstLink(propertyInfo.Name, obj => propertyInfo.GetValue(obj)));
                }
                return base.VisitMember(node);
            }
        }
    }
}