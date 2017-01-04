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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
{
    public class UpdateRepresentationActionFixture
    {
        private Mock<IRepresentationRequestParser> _requestParserStub;
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IApiResponseFactory> _apiResponseFactoryStub;
        private Mock<IRepresentationResponseParser> _responseParserStub;
        private Mock<IParametersValidator> _parametersValidatorStub;
        private Mock<IErrorResponseFactory> _errorResponseFactoryStub;
        private IFilterParser _filterParser;
        private IUpdateRepresentationAction _updateRepresentationAction;

        [Fact]
        public async Task When_Passing_Null_Or_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute(null, null, null, "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute(string.Empty, null, null, "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute("representation_id", null, null, "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute("representation_id", new JObject(), null, "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute("representation_id", new JObject(), string.Empty, "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute("representation_id", new JObject(), "schema_id", "http://localhost/{id}", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateRepresentationAction.Execute("representation_id", new JObject(), "schema_id", "http://localhost/{id}", string.Empty));
        }

        [Fact]
        public async Task When_Representation_Doesnt_Exist_Then_404_Is_Returned()
        {
            // ARRANGE
            const string identifier = "id";
            InitializeFakeObjects();
            _requestParserStub.Setup(r => r.Parse(It.IsAny<JObject>(), It.IsAny<string>(), CheckStrategies.Strong))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = true
                }));
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult((Representation)null));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.NotFound,
                string.Format(ErrorMessages.TheResourceDoesntExist, identifier)))
                .Returns(new ApiActionResult
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                });

            // ACT
            var result = await _updateRepresentationAction.Execute(identifier, new JObject(), "schema_id", "http://localhost/{id}", "resource_type");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task When_Trying_To_Update_Immutable_Parameter_Then_400_Is_Returned()
        {
            // ARRANGE
            const string identifier = "id";
            HttpStatusCode code = HttpStatusCode.Accepted;
            string mutability = string.Empty,
                message = string.Empty;
            InitializeFakeObjects();
            _requestParserStub.Setup(r => r.Parse(It.IsAny<JObject>(), It.IsAny<string>(), CheckStrategies.Strong))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = true,
                    Representation = new Representation
                    {
                        Attributes = new[]
                        {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                            {
                                Name = "id",
                                Mutability = Common.Constants.SchemaAttributeMutability.Immutable
                            }, "representation_id")
                        },
                        Id = identifier
                    }
                }));
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                        {
                            Name = "id",
                            Mutability = Common.Constants.SchemaAttributeMutability.Immutable
                        }, "representation_id_2")
                    },
                    Id = identifier
                }));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.BadRequest,
                It.IsAny<ErrorResponse>()))
                .Callback((HttpStatusCode c, ErrorResponse e) =>
                {
                    code = c;
                });
            _errorResponseFactoryStub.Setup(e => e.CreateError(It.IsAny<string>(), It.IsAny<HttpStatusCode>(), It.IsAny<string>()))
                .Callback((string m, HttpStatusCode c, string mu) => {
                    message = m;
                    mutability = mu;
                })
                .Returns(new ErrorResponse());

            // ACT
            await _updateRepresentationAction.Execute(identifier, new JObject(), "schema_id", "http://localhost/{id}", "resource_type");

            // ASSERT
            Assert.True(code == HttpStatusCode.BadRequest);
            Assert.True(message == string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, "id"));
            Assert.True(mutability == Common.Constants.ScimTypeValues.Mutability);
        }

        [Fact]
        public async Task When_Trying_To_Update_Unique_Parameter_Then_400_Is_Returned()
        {
            // ARRANGE
            const string identifier = "id";
            HttpStatusCode code = HttpStatusCode.Accepted;
            string mutability = string.Empty,
                message = string.Empty;
            InitializeFakeObjects();
            _requestParserStub.Setup(r => r.Parse(It.IsAny<JObject>(), It.IsAny<string>(), CheckStrategies.Strong))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = true,
                    Representation = new Representation
                    {
                        Attributes = new[]
                        {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                            {
                                Name = "id",
                                Mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                                Uniqueness = Common.Constants.SchemaAttributeUniqueness.Server,
                                Type = Common.Constants.SchemaAttributeTypes.String
                            }, "representation_id_2")
                        },
                        Id = identifier
                    }
                }));
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "id",
                            Mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                            Uniqueness = Common.Constants.SchemaAttributeUniqueness.Server,
                            Type = Common.Constants.SchemaAttributeTypes.String
                        }, "representation_id_2")
                    },
                    Id = identifier
                }));
            _representationStoreStub.Setup(r => r.GetRepresentations(It.IsAny<string>()))
                .Returns(Task.FromResult((IEnumerable<Representation>)new List<Representation> { new Representation
                {
                    Attributes = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "id",
                            Mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                            Uniqueness = Common.Constants.SchemaAttributeUniqueness.Server,
                            Type = Common.Constants.SchemaAttributeTypes.String
                        }, "representation_id_2")
                    },
                    Id = Guid.NewGuid().ToString()
                }}));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.BadRequest,
                It.IsAny<ErrorResponse>()))
                .Callback((HttpStatusCode c, ErrorResponse e) =>
                {
                    code = c;
                });
            _errorResponseFactoryStub.Setup(e => e.CreateError(It.IsAny<string>(), It.IsAny<HttpStatusCode>(), It.IsAny<string>()))
                .Callback((string m, HttpStatusCode c, string mu) => {
                    message = m;
                    mutability = mu;
                })
                .Returns(new ErrorResponse());

            // ACT
            await _updateRepresentationAction.Execute(identifier, new JObject(), "schema_id", "http://localhost/{id}", "resource_type");

            // ASSERT
            Assert.True(code == HttpStatusCode.BadRequest);
            Assert.True(message == string.Format(ErrorMessages.TheAttributeMustBeUnique, "id"));
            Assert.True(mutability == Common.Constants.ScimTypeValues.Uniqueness);
        }

        [Fact]
        public void When_Representation_Cannot_Be_Updated_Then_500_Is_Returned()
        {
            // ARRANGE
            const string identifier = "id";
            var code = HttpStatusCode.Accepted;
            var message = string.Empty;
            InitializeFakeObjects();
            _requestParserStub.Setup(r => r.Parse(It.IsAny<JObject>(), It.IsAny<string>(), CheckStrategies.Strong))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = true,
                    Representation = new Representation
                    {
                        Attributes = new[]
                        {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                            {
                                Name = "id",
                                Mutability = Common.Constants.SchemaAttributeMutability.writeOnly,
                                Type = Common.Constants.SchemaAttributeTypes.String
                            }, "representation_id")
                        },
                        Id = identifier
                    }
                }));
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse
                        {
                            Name = "id",
                            Mutability = Common.Constants.SchemaAttributeMutability.writeOnly,
                            Type = Common.Constants.SchemaAttributeTypes.String
                        }, "representation_id_2")
                    },
                    Id = identifier
                }));
            _apiResponseFactoryStub.Setup(a => a.CreateError(HttpStatusCode.InternalServerError,
                It.IsAny<string>()))
                .Callback((HttpStatusCode c, string msg) =>
                {
                    code = c;
                    message = msg;
                });
            _representationStoreStub.Setup(r => r.UpdateRepresentation(It.IsAny<Representation>()))
                .Returns(Task.FromResult(false));

            // ACT
            _updateRepresentationAction.Execute(identifier, new JObject(), "schema_id", "http://localhost/{id}", "resource_type");

            // ASSERT
            Assert.True(code == HttpStatusCode.InternalServerError);
            Assert.True(message == ErrorMessages.TheRepresentationCannotBeUpdated);
        }

        private void InitializeFakeObjects()
        {
            _requestParserStub = new Mock<IRepresentationRequestParser>();
            _representationStoreStub = new Mock<IRepresentationStore>();
            _apiResponseFactoryStub = new Mock<IApiResponseFactory>();
            _responseParserStub = new Mock<IRepresentationResponseParser>();
            _parametersValidatorStub = new Mock<IParametersValidator>();
            _errorResponseFactoryStub = new Mock<IErrorResponseFactory>();
            _filterParser = new FilterParser();
            _updateRepresentationAction = new UpdateRepresentationAction(
                _requestParserStub.Object,
                _representationStoreStub.Object,
                _apiResponseFactoryStub.Object,
                _responseParserStub.Object,
                _parametersValidatorStub.Object,
                _errorResponseFactoryStub.Object,
                _filterParser);
        }
    }
}
