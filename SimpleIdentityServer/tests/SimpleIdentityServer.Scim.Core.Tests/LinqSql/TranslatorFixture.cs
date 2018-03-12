using SimpleIdentityServer.Scim.Db.EF.Extensions;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.LinqSql
{
    public class TranslatorFixture
    {
        [Fact]
        public void When_Filtering_User_On_UserName_Then_One_User_Is_Rturned()
        {
            var representations = new List<Representation>
            {
                new Representation
                {
                    Attributes = new List<RepresentationAttribute>
                    {
                        new RepresentationAttribute
                        {
                            SchemaAttribute = new SchemaAttribute
                            {
                                Name = "userName"
                            },
                            Value = "jsmith"
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("userName eq jsmith");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_FirstLetter_Then_One_User_Is_Returned()
        {
            var representations = new List<Representation>
            {
                new Representation
                {
                    Attributes = new List<RepresentationAttribute>
                    {
                        new RepresentationAttribute
                        {
                        SchemaAttribute = new SchemaAttribute
                        {
                            Name = "name"
                        },
                        Children = new List<RepresentationAttribute>
                        {
                            new RepresentationAttribute
                            {
                                SchemaAttribute = new SchemaAttribute
                                {
                                    Name = "firstName"
                                },
                                Children = new List<RepresentationAttribute>
                                {
                                    new RepresentationAttribute
                                    {
                                        SchemaAttribute = new SchemaAttribute
                                        {
                                            Name = "firstLetter"
                                        },
                                        Value = "NO"
                                    }
                                }
                            }
                        }
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name.firstName[firstLetter eq NO]");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_FirstName_And_LastName_Then_One_User_Is_Returned()
        {
            var representations = new List<Representation>
            {
                new Representation
                {
                    Attributes = new List<RepresentationAttribute>
                    {
                        new RepresentationAttribute
                        {
                            SchemaAttribute = new SchemaAttribute
                            {
                                Name = "firstName"
                            },
                            Value = "tom"
                        },
                        new RepresentationAttribute
                        {
                            SchemaAttribute = new SchemaAttribute
                            {
                                Name = "lastName"
                            },
                            Value = "tim"
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("firstName eq tom and lastName eq tim");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);

        }

        [Fact]
        public void When_Filtering_On_FirstName_Or_LastName_Then_One_User_Is_Returned()
        {
            var representations = new List<Representation>
            {
                new Representation
                {
                    Attributes = new List<RepresentationAttribute>
                    {
                        new RepresentationAttribute
                        {
                            SchemaAttribute = new SchemaAttribute
                            {
                                Name = "name"
                            },
                            Children = new List<RepresentationAttribute>
                            {
                                new RepresentationAttribute
                                {
                                    SchemaAttribute = new SchemaAttribute
                                    {
                                        Name = "firstName"
                                    },
                                    Value = "thierry"
                                }
                            }
                        }
                    }
                }
            };
            
            var queryableAttrs = representations.AsQueryable();
                        
            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("(name.firstName eq thierry) or (name.lastName eq lokit)");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }
    }
}
