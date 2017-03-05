using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

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
        public static string GetPropertyName(this Expression<Func<object>> exp)
            => exp.Body.GetPropertyName();

        private static string GetPropertyName(this Expression exp)
        {
            var access = exp as MemberExpression;
            var prop = access?.Member as PropertyInfo;
            return prop?.Name;
        }
        public static List<string> GetUniquePropertyNames<TTarget, T2>(this Expression<Action<TTarget, T2>> exp)
        {
            var myVisitor = new NameVisitor(typeof(TTarget));
            myVisitor.Visit(exp.Body);
            return myVisitor.Names;
        }

        private sealed class NameVisitor : ExpressionVisitor
        {
            [NotNull]
            private readonly Type _filter;
            public readonly List<string> Names = new List<string>();

            public NameVisitor([NotNull] Type filter)
            {
                _filter = filter;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo?.DeclaringType == _filter)
                    Names.Add(propertyInfo.Name);
                return base.VisitMember(node);
            }
        }

    }
}