using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Web.Infastructure.Extensions
{
    public static class Queryable
    {
       // public static IOrderedQueryable<TSource> OrderBy<TSource>(
       //this IQueryable<TSource> query, string propertyName)
       // {
       //     var entityType = typeof(TSource);

       //     //Create x=>x.PropName
       //     var propertyInfo = entityType.GetProperty(propertyName);
       //     ParameterExpression arg = Expression.Parameter(entityType, "x");
       //     MemberExpression property = Expression.Property(arg, propertyName);
       //     var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

       //     //Get System.Linq.Queryable.OrderBy() method.
       //     var enumarableType = typeof(System.Linq.Queryable);
       //     var method = enumarableType.GetMethods()
       //          .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
       //          .Where(m =>
       //          {
       //              var parameters = m.GetParameters().ToList();
       //      //Put more restriction here to ensure selecting the right overload                
       //      return parameters.Count == 2;//overload that has 2 parameters
       //  });
       //     //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
       //     MethodInfo genericMethod = method.Single()
       //          .MakeGenericMethod(entityType, propertyInfo.PropertyType);

       //     /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
       //       Note that we pass the selector as Expression to the method and we don't compile it.
       //       By doing so EF can extract "order by" columns and generate SQL for it.*/
       //     var newQuery = (IOrderedQueryable<TSource>)genericMethod
       //          .Invoke(genericMethod, new object[] { query, selector });
       //     return newQuery;
       // }

        public static IOrderedQueryable<TSource> OrderByDesc<TSource>(
       this IQueryable<TSource> query, string propertyName)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderByDescending" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
             //Put more restriction here to ensure selecting the right overload                
             return parameters.Count == 2;//overload that has 2 parameters
         }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IQueryable Where<TSource>(
this IQueryable<TSource> query, string propertyName, object value)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            Expression valueExpression = Expression.Constant(value);
            var selector = Expression.Lambda(Expression.Equal(
                property, valueExpression
                ), new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "Where" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Last();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            //var newQuery = (IOrderedQueryable<TSource>)genericMethod
            //     .Invoke(genericMethod, new object[] { query, selector });

            return query.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", new Type[] { query.ElementType, selector.Body.Type },  query.Expression, Expression.Quote(selector) ));
        }

    }
}