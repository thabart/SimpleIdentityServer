using SimpleIdentityServer.Scim.Core.Models;
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
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "userName" })
                    {
                        Value = "val"
                    }
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
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "userName" })
                    {
                        Value = "val"
                    },
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "jobTitle" })
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
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "userName" })
                    {
                        Value = "val"
                    },
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "jobTitle" })
                    {
                        Value = "job"
                    },
                    new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "streetNumber"})
                    {
                        Parent = new RepresentationAttribute(new Common.DTOs.SchemaAttributeResponse { Name = "adr" })
                        {
                        },
                        Value = "100"
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
