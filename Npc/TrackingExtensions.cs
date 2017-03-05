using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using static System.Reflection.BindingFlags;
using Expression = System.Linq.Expressions.Expression;

namespace Npc
{
    public static class TrackingExtensions
    {
        public static ValueObserver<TResult> Track<TSource, TResult>(this TSource source, Expression<Func<TSource, TResult>> pathExpression)
        {
            return new ValueObserver<TResult>(Chain(source, source.GetLinks(pathExpression)));
        }
        public static SetObserver<TItem> TrackSet<TSource, TItem>(this TSource source, Expression<Func<TSource, ObservableCollection<TItem>>> pathExpression)
        {
            return new SetObserver<TItem>(Chain(source, source.GetLinks(pathExpression)));
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
                    var dependencyPropertyDeclaration = propertyInfo.DeclaringType?
                        .GetField(propertyInfo.Name+"Property", Public | Static)?
                        .GetValue(null) as DependencyProperty;
                    if (dependencyPropertyDeclaration != null)
                        Links.Add(new DependencyPropertyLink(dependencyPropertyDeclaration));
                    else if (typeof(INotifyPropertyChanged).IsAssignableFrom(node.Expression.Type))
                        Links.Add(new NpcLink(propertyInfo.PropertyType, propertyInfo.Name));
                    else
                        Links.Add(new FunctionLink(propertyInfo.PropertyType, 
                            propertyInfo.Name, obj => propertyInfo.GetValue(obj)));
                }
                return base.VisitMember(node);
            }
        }
    }
}