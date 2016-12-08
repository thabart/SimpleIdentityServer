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
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Jwks.Operations
{
    public sealed class RotateJsonWebKeysOperationFixture
    {
        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryStub;
        private IRotateJsonWebKeysOperation _rotateJsonWebKeysOperation;

        [Fact]
        public async Task When_There_Is_No_JsonWebKeys_To_Rotate_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jsonWebKeyRepositoryStub.Setup(j => j.GetAllAsync())
                .Returns(() => Task.FromResult((ICollection<JsonWebKey>)null));

            // ACT
            var result = await _rotateJsonWebKeysOperation.Execute();

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task When_Rotating_Two_JsonWebKeys_Then_SerializedKeyProperty_Has_Changed()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string firstJsonWebKeySerializedKey = "firstJsonWebKeySerializedKey";
            const string secondJsonWebKeySerializedKey = "secondJsonWebKeySerializedKey";
            ICollection<JsonWebKey> jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = "1",
                    SerializedKey = firstJsonWebKeySerializedKey
                },
                new JsonWebKey
                {
                    Kid = "2",
                    SerializedKey = secondJsonWebKeySerializedKey
                }
            };
            _jsonWebKeyRepositoryStub.Setup(j => j.GetAllAsync())
                .Returns(() => Task.FromResult(jsonWebKeys));

            // ACT
            var result = await _rotateJsonWebKeysOperation.Execute();

            // ASSERT
            _jsonWebKeyRepositoryStub.Verify(j => j.UpdateAsync(It.IsAny<JsonWebKey>()));
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _jsonWebKeyRepositoryStub = new Mock<IJsonWebKeyRepository>();
            _rotateJsonWebKeysOperation = new RotateJsonWebKeysOperation(_jsonWebKeyRepositoryStub.Object);
        }
    }
}
