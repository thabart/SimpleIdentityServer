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
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Factories;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Tests.Fake;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Helpers
{
    public class JsonWebKeyHelperFixture
    {
        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterStub;

        private Mock<IHttpClientFactory> _httpClientFactoryStub;

        private IJsonWebKeyHelper _jsonWebKeyHelper;

        #region Exceptions

        [Fact]
        public void When_Passing_No_Kid_To_GetJsonWebKey_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _jsonWebKeyHelper.GetJsonWebKey(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public void When_Passing_No_Uri_To_GetJsonWebKey_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _jsonWebKeyHelper.GetJsonWebKey("kid", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_JsonWebKey_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var uri = new Uri(url);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("json")
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _jsonWebKeyHelper.GetJsonWebKey(kid, uri)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Requesting_JsonWeb_Key_Then_Its_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var uri = new Uri(url);
            var jsonWebKeySet = new JsonWebKeySet();
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = kid
                }
            };
            var json = jsonWebKeySet.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = await _jsonWebKeyHelper.GetJsonWebKey(kid, uri).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.Kid == kid);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jsonWebKeyConverterStub = new Mock<IJsonWebKeyConverter>();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _jsonWebKeyHelper = new JsonWebKeyHelper(_jsonWebKeyConverterStub.Object,
                _httpClientFactoryStub.Object);
        }
    }
}
