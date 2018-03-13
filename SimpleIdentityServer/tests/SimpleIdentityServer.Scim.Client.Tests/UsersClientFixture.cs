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
using SimpleIdentityServer.Scim.Db.EF.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    public class UsersClientFixture : IClassFixture<TestScimServerFixture>
    {
        private const string baseUrl = "http://localhost:5555";
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private readonly TestScimServerFixture _testScimServerFixture;
        private IUsersClient _usersClient;

        public UsersClientFixture(TestScimServerFixture testScimServerFixture)
        {
            _testScimServerFixture = testScimServerFixture;
        }

        [Fact]
        public async Task When_Inset_Complex_Users_Then_Information_Are_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jArr = new JArray();
            jArr.Add("a1");
            jArr.Add("a2");
            var jObj = new JObject();
            jObj.Add(Common.Constants.NameResponseNames.MiddleName, "middlename");
            jObj.Add(Common.Constants.NameResponseNames.GivenName, "givename");
            var complexArr = new JArray();
            var complexObj = new JObject();
            complexObj.Add("test", "test2");
            complexArr.Add(complexObj);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testScimServerFixture.Client);
            var firstResult = await _usersClient.AddUser(baseUrl)
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, "username"))
                .AddAttribute(new JProperty("arr", jArr))
                .AddAttribute(new JProperty("date", DateTime.UtcNow))
                .AddAttribute(new JProperty("age", 23))
                .AddAttribute(new JProperty("complexarr", complexArr))
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.Name, jObj))
                .Execute();
            var id = firstResult.Content["id"].ToString();

            var firstSearch = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"arr co a1"
            });
            var secondSearch = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"complexarr[test eq test2]"
            });
            var thirdSearch = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"age le 23"
            });
            var newDate = DateTime.UtcNow.AddDays(2).ToUnix().ToString();
            var fourthSearch = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"date lt {newDate}"
            });

            Assert.NotNull(firstSearch);
            Assert.NotNull(secondSearch);
            Assert.NotNull(thirdSearch);
            Assert.NotNull(fourthSearch);

            var eightResult = await _usersClient.DeleteUser(baseUrl, id);
        }

        [Fact]
        public async Task When_Executing_Operations_On_Users_Then_No_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testScimServerFixture.Client);
            var patchOperation = new PatchOperationBuilder().SetType(PatchOperations.replace)
                .SetPath(Common.Constants.UserResourceResponseNames.UserName)
                .SetContent("new_username")
                .Build();
            var addEmailsOperation = new PatchOperationBuilder().SetType(PatchOperations.replace)
                .SetPath(Common.Constants.UserResourceResponseNames.Emails)
                .SetContent(JArray.Parse("[{'" + Common.Constants.MultiValueAttributeNames.Type + "' : 'work','" + Common.Constants.MultiValueAttributeNames.Value + "' : 'bjensen@example.com'}, {'" + Common.Constants.MultiValueAttributeNames.Type + "' : 'home','" + Common.Constants.MultiValueAttributeNames.Value + "' : 'bjensen@example.com'}]"))
                .Build();
            var removeEmailOperation = new PatchOperationBuilder().SetType(PatchOperations.remove)
                .SetPath("emails[type eq work]")
                .Build();

            // ACT : Create user
            var firstResult = await _usersClient.AddUser(baseUrl)
                .SetCommonAttributes("external_id")
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, "username"))
                .Execute();

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
            var id = firstResult.Content["id"].ToString();

            // ACT : Partial update user
            var secondResult = await _usersClient.PartialUpdateUser(baseUrl, id)
                .AddOperation(patchOperation)
                .Execute();

            // ASSERTS
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Content[Common.Constants.UserResourceResponseNames.UserName].ToString() == "new_username");

            // ACT : Update user
            var thirdResult = await _usersClient.UpdateUser(baseUrl, id)
                .SetCommonAttributes("new_external_id")
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, "other_username"))
                .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.Active, "false"))
                .Execute();

            // ASSERTS
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
            Assert.True(thirdResult.Content[Common.Constants.UserResourceResponseNames.UserName].ToString() == "other_username");
            var active = thirdResult.Content[Common.Constants.UserResourceResponseNames.Active].ToString();
            Assert.False(bool.Parse(active));
            Assert.True(thirdResult.Content[Common.Constants.IdentifiedScimResourceNames.ExternalId].ToString() == "new_external_id");

            // ACT : Add emails to the user
            var fourthResult = await _usersClient.PartialUpdateUser(baseUrl, id)
                .AddOperation(addEmailsOperation)
                .Execute();

            // ASSERTS
            Assert.NotNull(fourthResult);
            Assert.True(fourthResult.StatusCode == HttpStatusCode.OK);
            Assert.True(fourthResult.Content[Common.Constants.UserResourceResponseNames.Emails].Count() == 2);

            /*
            // ACT : Remove emails of the user
            var fifthResult = await _usersClient.PartialUpdateUser(baseUrl, id)
                .AddOperation(removeEmailOperation)
                .Execute();

            // ASSERTS
            Assert.NotNull(fifthResult);
            Assert.True(fifthResult.StatusCode == HttpStatusCode.OK);
            Assert.True(fifthResult.Content[Common.Constants.UserResourceResponseNames.Emails].Count() == 1);
            */

            // ACT : Add 10 users
            for (int i = 0; i < 10; i++)
            {
                await _usersClient.AddUser(baseUrl)
                   .SetCommonAttributes(Guid.NewGuid().ToString())
                   .AddAttribute(new JProperty(Common.Constants.UserResourceResponseNames.UserName, Guid.NewGuid().ToString()))
                   .Execute();
            }

            // ACT : Get 10 users
            var sixResult = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                StartIndex = 0,
                Count = 10
            });

            // ASSERTS
            Assert.NotNull(sixResult);
            var c = sixResult.Content["Resources"];
            Assert.True(sixResult.Content["Resources"].Count() == 10);

            // ACT : Get only emails
            var sevenResult = await _usersClient.SearchUsers(baseUrl, new SearchParameter
            {
                Filter = "emails[type pr]",
                Attributes = new[] { "emails.type", "emails.value", "emails.display", "userName" }
            });

            // ASSERTS
            Assert.NotNull(sevenResult);

            // ACT : Remove the user
            var eightResult = await _usersClient.DeleteUser(baseUrl, id);

            // ASSERTS
            Assert.NotNull(eightResult);
            Assert.True(eightResult.StatusCode == HttpStatusCode.NoContent);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _usersClient = new UsersClient(_httpClientFactoryStub.Object);
        }
    }
}
