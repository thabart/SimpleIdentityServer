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
using SimpleIdentityServer.Scim.Client.Builders;
using SimpleIdentityServer.Scim.Client.Factories;
using System.Linq;
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
            var arr = JArray.Parse("[{'" + Common.Constants.MultiValueAttributeNames.Type + "' : 'group','" + Common.Constants.MultiValueAttributeNames.Value + "' : 'value'},{'" + Common.Constants.MultiValueAttributeNames.Type + "' : 'group2','" + Common.Constants.MultiValueAttributeNames.Value + "' : 'value2'}]");
            var patchOperation = new PatchOperationBuilder().SetType(PatchOperations.replace)
                .SetPath(Common.Constants.GroupResourceResponseNames.Members)
                .SetContent(arr)
                .Build();
            var removeGroupOperation = new PatchOperationBuilder().SetType(PatchOperations.remove)
                .SetPath("members[type eq group2]")
                .Build();
            var addGroupOperation = new PatchOperationBuilder().SetType(PatchOperations.add)
                .SetPath("members")
                .SetContent(JArray.Parse("[{'" + Common.Constants.MultiValueAttributeNames.Type + "' : 'group3','" + Common.Constants.MultiValueAttributeNames.Value + "' : 'value3'}]"))
                .Build();
            var updateGroupOperation = new PatchOperationBuilder().SetType(PatchOperations.replace)
                .SetPath("members[type eq group3].value")
                .SetContent("new_value")
                .Build();

            // ACT : Create group
            var firstResult = await _groupsClient.AddGroup(baseUrl).SetCommonAttributes("external_id").Execute().ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
            var id = firstResult.Content["id"].ToString();

            // ACT : Get group
            var secondResult = await _groupsClient.GetGroup(baseUrl, id).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(secondResult);
            Assert.True(secondResult.StatusCode == HttpStatusCode.OK);
            Assert.True(secondResult.Content["id"].ToString() == id);

            // ACT : Update group
            var thirdResult = await _groupsClient.UpdateGroup(baseUrl, id)
                .SetCommonAttributes("other_id")
                .AddAttribute(new JProperty(Common.Constants.GroupResourceResponseNames.DisplayName, "display_name"))
                .Execute().ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
            Assert.True(thirdResult.Content["id"].ToString() == id);
            Assert.True(thirdResult.Content[Common.Constants.GroupResourceResponseNames.DisplayName].ToString() == "display_name");
            Assert.True(thirdResult.Content[Common.Constants.IdentifiedScimResourceNames.ExternalId].ToString() == "other_id");

            // ACT : Partial update group
            var fourthResult = await _groupsClient.PartialUpdateGroup(baseUrl, id)
                .AddOperation(patchOperation)
                .Execute().ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(fourthResult);
            Assert.True(fourthResult.StatusCode == HttpStatusCode.OK);
            Assert.True(fourthResult.Content["id"].ToString() == id);
            Assert.True(fourthResult.Content[Common.Constants.GroupResourceResponseNames.Members][0][Common.Constants.MultiValueAttributeNames.Type].ToString() == "group");
            Assert.True(fourthResult.Content[Common.Constants.GroupResourceResponseNames.Members][0][Common.Constants.MultiValueAttributeNames.Value].ToString() == "value");

            // ACT : Remove group2
            var fifthResult = await _groupsClient.PartialUpdateGroup(baseUrl, id)
                .AddOperation(removeGroupOperation)
                .Execute().ConfigureAwait(false);

            Assert.NotNull(fifthResult != null);
            Assert.True(fifthResult.Content[Common.Constants.GroupResourceResponseNames.Members].Count() == 1);

            // ACT : Add group3
            var sixResult = await _groupsClient.PartialUpdateGroup(baseUrl, id)
                .AddOperation(addGroupOperation)
                .Execute().ConfigureAwait(false);

            Assert.NotNull(sixResult);
            Assert.True(sixResult.Content[Common.Constants.GroupResourceResponseNames.Members].Count() == 2);

            // ACT : Update the group3 type (immutable property)
            var sevenResult = await _groupsClient.PartialUpdateGroup(baseUrl, id)
                .AddOperation(updateGroupOperation)
                .Execute().ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(sevenResult);
            Assert.True(sevenResult.StatusCode == HttpStatusCode.BadRequest);

            // ACT : Add ten groups
            for(var i = 0; i < 10; i++)
            {
                await _groupsClient.AddGroup(baseUrl).SetCommonAttributes("external_id").Execute().ConfigureAwait(false);
            }

            // ACT : Get 10 groups
            var eightResult = await _groupsClient.SearchGroups(baseUrl, new SearchParameter
            {
                StartIndex = 1,
                Count = 10
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(eightResult);
            Assert.True(eightResult.Content["Resources"].Count() == 10);


            // ACT : Get only members
            var nineResult = await _groupsClient.SearchGroups(baseUrl, new SearchParameter
            {
                Filter = "members[type pr]",
                Attributes = new[] { "members.type" }
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(nineResult);

            // ACT : Remove group
            var thenResult = await _groupsClient.DeleteGroup(baseUrl, id).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(thenResult);
            Assert.True(thenResult.StatusCode == HttpStatusCode.NoContent);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _groupsClient = new GroupsClient(_httpClientFactoryStub.Object);
        }
    }
}
