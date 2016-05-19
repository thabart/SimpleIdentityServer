#region copyright
// Copyright 2015 Habart Thierry
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
using Newtonsoft.Json;
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using SimpleIdentityServer.UmaManager.Client.Factory;
using SimpleIdentityServer.UmaManager.Client.Operation;
using SimpleIdentityServer.UmaManager.Client.Unit.Tests.Fake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.UmaManager.Client.Unit.Tests.Operation
{
    public class SearchOperationsActionFixture
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;

        private ISearchOperationsAction _searchOperationsAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _searchOperationsAction.ExecuteAsync(null, null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _searchOperationsAction.ExecuteAsync("resource_set_id", null));
        }

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Searching_Operations_Then_List_Is_Returned()
        {
            // ARRANGE
            const string applicationName = "application_name";
            const string operationName = "operation_name";
            const string resourceSetId = "resource_set_id";
            const string url = "http://localhost/operations/";
            InitializeFakeObjects();
            var operations = new List<SearchOperationResponse>
            {
                new SearchOperationResponse
                {
                    ApplicationName = applicationName,
                    OperationName = operationName,
                    ResourceSetId = resourceSetId
                }
            };
            var json = JsonConvert.SerializeObject(operations);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var client = new HttpClient(fakeHttpHandler);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(client);

            // ACT
            var result = await _searchOperationsAction.ExecuteAsync(resourceSetId, new Uri(url));

            // ASSERTS
            var request = fakeHttpHandler.GetRequest();
            Assert.NotNull(result);
            Assert.True(result.First().OperationName == operationName);
            Assert.True(result.First().ApplicationName == applicationName);
            Assert.True(request.RequestUri.AbsoluteUri == $"http://localhost/operations?resourceSet={resourceSetId}");
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _searchOperationsAction = new SearchOperationsAction(_httpClientFactoryStub.Object);
        }
    }
}
