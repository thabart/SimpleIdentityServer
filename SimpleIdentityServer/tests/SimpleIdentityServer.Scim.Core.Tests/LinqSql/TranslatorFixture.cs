using SimpleIdentityServer.Scim.Core.EF.Extensions;
using SimpleIdentityServer.Scim.Core.EF.Models;
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

        [Fact]
        public void When_Filtering_On_Existing_FirstName_Then_One_User_Is_Returned()
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
                            Value = null
                        }
                    }
                },
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
                            Value = "name"
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name pr");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_Age_Lower_Than_26_Then_One_User_Is_Returned()
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
                                Name = "age"
                            },
                            ValueNumber = 26
                        }
                    }
                },
                new Representation
                {
                    Attributes = new List<RepresentationAttribute>
                    {
                        new RepresentationAttribute
                        {
                            SchemaAttribute = new SchemaAttribute
                            {
                                Name = "age"
                            },
                            ValueNumber = 25
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("age lt 26");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_Email_Then_One_User_Is_Returned()
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
                                Name = "email",
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                MultiValued = true
                            },
                            Values = new List<RepresentationAttributeValue>
                            {
                                new RepresentationAttributeValue
                                {
                                    Value = "email3"
                                },
                                new RepresentationAttributeValue
                                {
                                    Value = "email1"
                                }
                            }
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("email co email1");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_Email_With_Str_Contains_Then_One_User_Is_Returned()
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
                                Name = "email",
                                Type = Common.Constants.SchemaAttributeTypes.String
                            },
                            Value = "email1"
                        }
                    }
                }
            };
            var queryableAttrs = representations.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("email co e");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_ComplexAttributes_Then_Representation_Is_Returned()
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
                                Name = "complexarr",
                                Type = Common.Constants.SchemaAttributeTypes.Complex,
                                MultiValued = true
                            },
                            Children = new List<RepresentationAttribute>
                            {
                                new RepresentationAttribute
                                {
                                    Children = new List<RepresentationAttribute>
                                    {
                                        new RepresentationAttribute
                                        {
                                            SchemaAttribute = new SchemaAttribute
                                            {
                                                Name = "test",
                                                Type = Common.Constants.SchemaAttributeTypes.String
                                            },
                                            Value = "value"
                                        },
                                        new RepresentationAttribute
                                        {
                                            SchemaAttribute = new SchemaAttribute
                                            {
                                                Name = "type",
                                                Type = Common.Constants.SchemaAttributeTypes.String
                                            },
                                            Value = "type"
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
            var parsed = p.Parse("complexarr[test eq value and type eq type2 or test eq value]");
            var evalutedExpr = parsed.EvaluateFilter(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Selecting_FirstName_Then_Attribute_Is_Returned()
        {
            var representations = new List<Representation>
            {
                new Representation
                {
                    Id = "repid",
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
            var representationAttributes = new List<RepresentationAttribute>
            {
                new RepresentationAttribute
                {
                     Id = "id",
                     SchemaAttribute = new SchemaAttribute
                     {
                         Name = "name"
                     },
                     RepresentationId = "repid"
                },
                new RepresentationAttribute
                {
                    Id = "subid",
                    SchemaAttribute = new SchemaAttribute
                    {
                        Name = "firstName"
                    },
                    Value = "thierry",
                    RepresentationAttributeIdParent = "id",
                    RepresentationId = "repid"
                }
            };
            
            var queryableReprs = representations.AsQueryable();
            var queryableAttrs = representationAttributes.AsQueryable();


            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name.firstName");
            var evalutedExpr = parsed.EvaluateSelection(queryableReprs, queryableAttrs);

            var o = (IQueryable<RepresentationAttribute>)evalutedExpr.Compile().DynamicInvoke(queryableReprs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
            Assert.True(o.First().SchemaAttribute.Name == "firstName");
        }
    }
}
