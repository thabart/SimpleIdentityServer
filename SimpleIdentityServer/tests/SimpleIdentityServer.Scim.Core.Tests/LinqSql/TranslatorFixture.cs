using SimpleIdentityServer.Scim.Db.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using SimpleIdentityServer.Scim.Db.EF.Extensions;

namespace SimpleIdentityServer.Scim.Core.Tests.LinqSql
{
    public class TranslatorFixture
    {
        [Fact]
        public void WhenExecuteSelectName()
        {
            var firstPerson = new Representation();
            var secondPerson = new Representation();
            var firstAttrs = new List<RepresentationAttribute>();
            firstAttrs.Add(new RepresentationAttribute
            {
                SchemaAttribute = new SchemaAttribute
                {
                    Name = "userName"
                },
                Value = "bjensen"
            });
            firstPerson.Attributes = firstAttrs;

            var secondAttrs = new List<RepresentationAttribute>();
            secondAttrs.Add(new RepresentationAttribute
            {
                SchemaAttribute = new SchemaAttribute
                {
                    Name = "userName"
                },
                Value = "jsmith"
            });
            secondPerson.Attributes = secondAttrs;

            // TODO : GENERATE THE TREE VIEW BASED ON THE INSTRUCTION.

            /*
            Expression.Property(Expression.Constant())
            var equality = Expression.Equals();
            */
            var resources = new List<Representation> { firstPerson, secondPerson };
            var p = new SimpleIdentityServer.Scim.Core.Parsers.FilterParser();
            var parsed = p.Parse("userName eq \"john\"");
            parsed.Evaluate(resources);

            var rr = resources.Where(r => r.Attributes.Any(a => a.SchemaAttribute.Name == "userName" && a.Value == "jsmith"));

            var resourceParameter = Expression.Parameter(typeof(SimpleIdentityServer.Scim.Core.Models.Representation), "r");
            var resourceAttributes = Expression.PropertyOrField(resourceParameter, "Attributes");

            var resourceAttrParameterExpr = Expression.Parameter(typeof(SimpleIdentityServer.Scim.Core.Models.RepresentationAttribute), "a");
            var propertyName = Expression.Property(resourceAttrParameterExpr, "Name");
            var propertyValue = Expression.Property(resourceAttrParameterExpr, "Value");

            var equalName = Expression.Equal(propertyName, Expression.Constant("userName"));
            var equalValue = Expression.Equal(propertyValue, Expression.Constant("jsmith"));
            var andExpression = Expression.And(equalName, equalValue);

            var predicate = Expression.Lambda<Func<SimpleIdentityServer.Scim.Core.Models.RepresentationAttribute, bool>>(andExpression, resourceAttrParameterExpr);
            var anyR = Expression.Call(typeof(Enumerable), "Any", new[] { typeof(SimpleIdentityServer.Scim.Core.Models.RepresentationAttribute) }, resourceAttributes, predicate);
            var call = Expression.Lambda<Func< SimpleIdentityServer.Scim.Core.Models.Representation, bool>>(anyR, resourceParameter);

            var enumarableType = typeof(Queryable);
            var genericMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == "Where" && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(typeof(SimpleIdentityServer.Scim.Core.Models.Representation));
            var where = Expression.Call(genericMethod, Expression.Constant(resources), call);

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<SimpleIdentityServer.Scim.Core.Models.Representation>), "f");
            var finalSelectRequestBody = Expression.Lambda(where, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(rr);
            // Expression.Lambda();
            string s = "";
        }

        public static IQueryable<object> Filter<TSource>(IQueryable<TSource> source, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return null;
        }
    }
}
