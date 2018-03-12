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

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }

        [Fact]
        public void When_Filtering_On_FirstLetter_Then_One_User_Is_Returned()
        {
            var attrs = new List<RepresentationAttribute>();
            attrs.Add(new RepresentationAttribute
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
            });
            var queryableAttrs = attrs.AsQueryable();

            var p = new Core.Parsers.FilterParser();
            var parsed = p.Parse("name.firstName[firstLetter eq NO]");
            var evalutedExpr = parsed.Evaluate(queryableAttrs);

            var o = (IQueryable<object>)evalutedExpr.Compile().DynamicInvoke(queryableAttrs);

            Assert.NotNull(o);
            Assert.True(o.Count() == 1);
        }
    }
}
