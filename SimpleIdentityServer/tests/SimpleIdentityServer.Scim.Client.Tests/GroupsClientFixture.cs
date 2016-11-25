#region copyright
// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Client.Factories;
using SimpleIdentityServer.Scim.Client.Groups;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    public class GroupsClientFixture : IClassFixture<TestScimServerFixture>
    {
        private readonly TestScimServerFixture _testScimServerFixture;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IGroupsClient _groupsClient;

        public GroupsClientFixture(TestScimServerFixture testScimServerFixture)
        {
            _testScimServerFixture = testScimServerFixture;
        }

        [Fact]
        public async Task When_Executing_Operations_On_Groups_Then_No_Exceptions_Are_Thrown()
        {
            const string baseUrl = "http://localhost:5555";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testScimServerFixture.Client);

            // ACT : Create group
            var firstResult = await _groupsClient.AddGroup(baseUrl).SetCommonAttributes("external_id").Execute();

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
            var id = firstResult.Content["id"].ToString();

            // ACT : Get group
            var secondResult = await _groupsClient.GetGroup(baseUrl, id);

            // ASSERTS
            Assert.NotNull(secondResult);
            Assert.True(secondResult.StatusCode == HttpStatusCode.OK);
            Assert.True(secondResult.Content["id"].ToString() == id);

            // ACT : Update group
            var thirdResult = await _groupsClient.UpdateGroup(baseUrl, id)
                .SetCommonAttributes("other_id")
                .AddAttribute(new JProperty(Common.Constants.GroupResourceResponseNames.DisplayName, "display_name"))
                .Execute();

            // ASSERTS
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
            Assert.True(thirdResult.Content["id"].ToString() == id);
            Assert.True(thirdResult.Content[Common.Constants.GroupResourceResponseNames.DisplayName].ToString() == "display_name");
            Assert.True(thirdResult.Content[Common.Constants.IdentifiedScimResourceNames.ExternalId].ToString() == "other_id");

            // ACT : Remove group
            var fourthResult = await _groupsClient.DeleteGroup(baseUrl, id);

            // ASSERTS
            Assert.NotNull(fourthResult);
            Assert.True(fourthResult.StatusCode == HttpStatusCode.NoContent);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _groupsClient = new GroupsClient(_httpClientFactoryStub.Object);
        }
    }
}
