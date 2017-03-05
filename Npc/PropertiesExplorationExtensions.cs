using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Npc
{
    public static class PropertiesExplorationExtensions
    {
        public static IEnumerable<Expression<Func<object>>> ExtractSubexpressions<T>(this Expression<Func<T>> exp)
        {
            var v = new AccessListVisitor(exp.Body);
            v.Visit(exp.Body);
            return v.All.Select(e => Expression.Lambda<Func<object>>(e, false));
        }
        public static IEnumerable<string> ExtractPropertyNames<TSource, TResult>(this Expression<Func<TSource, TResult>> exp)
        {
            var v = new AccessListVisitor(exp.Body);
            v.Visit(exp.Body);
            return v.All.Select(e => e.GetPropertyName());
        }
        public static IEnumerable<string> ExtractPropertyNames<T>(this Expression<Func<T>> exp)
        {
            var v = new AccessListVisitor(exp.Body);
            v.Visit(exp.Body);
            return v.All.Select(e => e.GetPropertyName());
        }

        sealed class AccessListVisitor : ExpressionVisitor
        {
            public AccessListVisitor(Expression root)
            {
                All.Push(root);
            }

            public Stack<Expression> All { get; } = new Stack<Expression>();

            protected override Expression VisitMember(MemberExpression node)
            {
                All.Push(node.Expression);
                return base.VisitMember(node);
            }
        }

        private static string GetPropertyName(this Expression exp)
        {
            var access = exp as MemberExpression;
            var prop = access?.Member as PropertyInfo;
            return prop?.Name;
        }
    }
}