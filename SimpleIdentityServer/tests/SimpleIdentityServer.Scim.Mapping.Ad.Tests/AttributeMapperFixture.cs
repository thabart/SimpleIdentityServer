using Moq;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Tests
{
    public class AttributeMapperFixture
    {
        // [Fact]
        public async Task When_Get_Representation_Attribute_Then_Ok_Is_Returned()
        {
            var representation = new Representation
            {
                Attributes = new List<RepresentationAttribute>
                {
                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "userName" }, "thabart"),
                    new RepresentationAttribute(new SchemaAttributeResponse { Name = "lastName", Id = "1" })
                    {
                    }
                }
            };
            var configurationStore = new Mock<IConfigurationStore>();
            var mappingStore = new Mock<IMappingStore>();
            configurationStore.Setup(s => s.GetConfiguration()).Returns(new AdConfiguration
            {
                IpAdr = "127.0.0.1",
                Port = 10389,
                UserFilter = "(uid=${userName})",
                DistinguishedName = "ou=system",
                Username = "uid=admin,ou=system",
                Password = "secret"
            });
            mappingStore.Setup(m => m.GetMapping(It.IsAny<string>())).Returns(Task.FromResult(new AdMapping
            {
                AdPropertyName = "sn",
                AttributeId = "1"
            }));

            var attributeMapper = new AttributeMapper(configurationStore.Object, mappingStore.Object, new UserFilterParser());

            await attributeMapper.Map(representation);

            string s2 = "";
        }
    }
}
