using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Npc
{
    public static class ObservableExtensions
    {
        public static OneValue<TResult> Track<TSource, TResult>(this TSource source, Expression<Func<TSource, TResult>> pathExpression)
            where TSource : INotifyPropertyChanged
        {
            return new OneValue<TResult>(Chain(source, source.GetLinks(pathExpression)));
        }

        public static List<ILink> GetLinks<TSource, TResult>(this TSource _, Expression<Func<TSource, TResult>> pathExpression) 
        {
            var visitor = new Visitor();
            visitor.Visit(pathExpression.Body);
            visitor.Links.Reverse();
            return visitor.Links;
        }

        private static ILink Chain(object source, List<ILink> links)
        {
            if (links.Count == 0) throw new ArgumentOutOfRangeException(nameof(links), "Track does not accept paths of length 0");
            links[0].ChangeSource(source);
            if (links.Count == 1) return links[0];
            return links.Skip(count: 1).Aggregate(links[0], Combine);
        }
        private static ILink Combine(ILink source, ILink result)
        {
            var resourceContainer = (ResourceContainer)result;
            resourceContainer.Resources.Add(source.Dispose);
            source.Subscribe(result.ChangeSource);
            result.ChangeSource(source.Value);
            return result;
        }

        sealed class Visitor : ExpressionVisitor
        {
            public List<ILink> Links { get; } = new List<ILink>();
            protected override Expression VisitMember(MemberExpression node)
            {
                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(node.Expression.Type))
                        Links.Add(new NpcLink(propertyInfo.PropertyType, propertyInfo.Name));
                    else
                        Links.Add(new ConstLink(propertyInfo.PropertyType, 
                            propertyInfo.Name, obj => propertyInfo.GetValue(obj)));
                }
                return base.VisitMember(node);
            }
        }
    }
}