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
using SimpleIdentityServer.UmaManager.Client.Operation;
using System;
using Xunit;

namespace SimpleIdentityServer.UmaManager.Client.Unit.Tests.Operation
{
    public class OperationClientFixture
    {
        private Mock<ISearchOperationsAction> _searchOperationsActionStub;

        private IOperationClient _operationClient;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _operationClient.Search(null, string.Empty));
        }

        [Fact]
        public void When_Passing_Invalid_Url_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string url = "not_valid_url";
            InitializeFakeObjects();

            // ACT & ASSERT
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _operationClient.Search(null, url));
            Assert.True(exception.Result.Message == $"the url {url} is not well formed");
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Searching_Operations_Then_List_Is_Returned()
        {
            // ARRANGE
            const string resourceSetId = "resource_set_id";
            const string url = "http://localhost";
            InitializeFakeObjects();

            // ACT
            var result = _operationClient.Search(resourceSetId, url).Result;

            // ASSERTS
            _searchOperationsActionStub.Verify(s => s.ExecuteAsync(resourceSetId, new Uri(url)));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _searchOperationsActionStub = new Mock<ISearchOperationsAction>();
            _operationClient = new OperationClient(_searchOperationsActionStub.Object);
        }

        #endregion
    }
}
