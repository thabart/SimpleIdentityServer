using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Tests
{
    public class UserFilterParserFixture
    {
        [Fact]
        public void When_Parse_Filter_And_Pass_UserName_Then_Filter_Is_Returned()
        {
            // ARRANGE
            var filterParser = new UserFilterParser();
            var representation = new Representation
            {
                Attributes = new List<RepresentationAttribute>
                {
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "userName" }, "val")
                }
            };

            // ACT
            var filter = filterParser.Parse("&(CN=${userName})", representation);

            // ASSERT
            Assert.Equal("&(CN=val)", filter);
        }

        [Fact]
        public void When_Parse_Filter_With_Two_Parameters_Then_Filter_Is_Returned()
        {
            // ARRANGE
            var filterParser = new UserFilterParser();
            var representation = new Representation
            {
                Attributes = new List<RepresentationAttribute>
                {
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "userName" }, "val"),
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "jobTitle" }, "job")
                    {
                        Value = "job"
                    }
                }
            };

            // ACT
            var filter = filterParser.Parse("&(CN=${userName})(jobTitle=${jobTitle})", representation);

            // ASSERT
            Assert.Equal("&(CN=val)(jobTitle=job)", filter);
        }

        [Fact]
        public void When_Parse_Filter_With_Three_Parameter_And_One_Complex_Parameter_Then_Filter_Is_Returned()
        {
            // ARRANGE
            var filterParser = new UserFilterParser();
            var representation = new Representation
            {
                Attributes = new List<RepresentationAttribute>
                {
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "userName" }, "val"),
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "jobTitle" }, "job"),
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "streetNumber"}, "100")
                    {
                        Parent = new RepresentationAttribute(new SchemaAttributeResponse { Name = "adr" })
                        {
                        }
                    }
                }
            };

            // ACT
            var filter = filterParser.Parse("&(CN=${userName})(jobTitle=${jobTitle})(streetNumber=${adr.streetNumber})", representation);

            // ASSERT
            Assert.Equal("&(CN=val)(jobTitle=job)(streetNumber=100)", filter);
        }
    }
}