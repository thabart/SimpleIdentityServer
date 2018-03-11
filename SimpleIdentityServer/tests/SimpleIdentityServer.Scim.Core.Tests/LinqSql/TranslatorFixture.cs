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
        public void When_Select_FirstName()
        {
            var representationAttributes = new List<RepresentationAttribute>
            {
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "firstName"
                        },
                        Value = "jsmith"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "lastName"
                        },
                        Value = "smooth"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "name"
                        },
                        Id = "id"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "firstName"
                        },
                        Value = "jhon",
                        RepresentationAttributeIdParent = "id"
                    }
            };
            var res = representationAttributes.Where(r => r.SchemaAttribute.Name == "name") // Select all the names + firstName
                .Join(representationAttributes,
                    post => post.Id,
                    z => z.RepresentationAttributeIdParent,
                    (a, b) => new { a = a, b = b })
                    .Where(b => b.b.SchemaAttribute.Name == "firstName")
                    .Select(b => b.b.Value).ToList();
            var queryableAttrs = representationAttributes.AsQueryable();
            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name.firstName");

            var evalutedExpr = parsed.Evaluate(queryableAttrs);

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = Expression.Lambda(evalutedExpr, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.First().ToString() == "jhon");
        }

        [Fact]
        public void When_Select_FirstName_Equals_To_John()
        {
            var representationAttributes = new List<RepresentationAttribute>
            {
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "firstName"
                        },
                        Value = "jsmith"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "lastName"
                        },
                        Value = "smooth"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "name"
                        },
                        Id = "id"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "name"
                        },
                        Id = "id_correct",
                        Children = new List<RepresentationAttribute>
                        {
                            new RepresentationAttribute
                            {
                                SchemaAttribute = new SchemaAttribute
                                {
                                    Name = "firstName"
                                },
                                Value = "john",
                                RepresentationAttributeIdParent = "id_correct"
                            }
                        }
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "firstName"
                        },
                        Value = "jhon",
                        RepresentationAttributeIdParent = "id"
                    },
                    new RepresentationAttribute
                    {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "firstName"
                        },
                        Value = "john",
                        RepresentationAttributeIdParent = "id_correct"
                    }
            };
            var res = representationAttributes.Where(r => r.SchemaAttribute.Name == "name") // Select all the names + firstName
                // .Where(a => a.Children.Any(c => c.SchemaAttribute.Name == "" && c.Value == ""))
                .Join(representationAttributes,
                    post => post.Id,
                    z => z.RepresentationAttributeIdParent,
                    (a, b) => new { a = a, b = b })
                    .Where(b => b.b.SchemaAttribute.Name == "firstName")
                    .Where(b => b.b.Value == "john")
                    .Select(b => b.b.Value).ToList();
            var queryableAttrs = representationAttributes.AsQueryable();
            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name[firstName eq john]");

            var evalutedExpr = parsed.Evaluate(queryableAttrs);

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = Expression.Lambda(evalutedExpr, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.First().ToString() == "john");
        }

        [Fact]
        public void When_Execute_Where_Instruction_On_UserName()
        {
            var attrs = new List<RepresentationAttribute>();
            attrs.Add(new RepresentationAttribute
            {
                SchemaAttribute = new SchemaAttribute
                {
                    Name = "userName"
                },
                Value = "bjensen"
            });
            attrs.Add(new RepresentationAttribute
            {
                SchemaAttribute = new SchemaAttribute
                {
                    Name = "userName"
                },
                Value = "jsmith"
            });
            var queryableAttrs = attrs.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("userName eq jsmith");
            var evalutedExpr = parsed.Evaluate(queryableAttrs);

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = Expression.Lambda(evalutedExpr, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }
    }
}
