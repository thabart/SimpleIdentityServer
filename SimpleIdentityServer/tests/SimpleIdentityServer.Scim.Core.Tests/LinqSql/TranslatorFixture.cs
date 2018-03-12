using SimpleIdentityServer.Scim.Db.EF.Extensions;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.LinqSql
{
    public class TranslatorFixture
    {
        [Fact]
        public void When_Select_All_FirstNames_Then_List_Is_Returned()
        {
            // ARRANGE
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
            var queryableAttrs = representationAttributes.AsQueryable();
            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name.firstName");

            // ACT
            var evalutedExpr = parsed.Evaluate(queryableAttrs);
            var finalSelectArg = Expression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = Expression.Lambda(evalutedExpr, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(queryableAttrs);

            // ASSERTS
            Assert.NotNull(o);
            Assert.True(o.First().ToString() == "jhon");
        }
        
        [Fact]
        public void When_Filtering_Users_On_FirstName_Then_One_User_Is_Returned()
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
                    }
            };
            var queryableAttrs = representationAttributes.AsQueryable();
            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name[firstName eq john]");

            var evalutedExpr = parsed.Evaluate(queryableAttrs);

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = Expression.Lambda(evalutedExpr, new ParameterExpression[] { finalSelectArg });
            var o = (IQueryable<object>)finalSelectRequestBody.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_User_On_UserName_Then_One_User_Is_Rturned()
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
