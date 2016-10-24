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
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.DTOs;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Parsers
{
    public class PatchRepresentationActionFixture
    {
        private Mock<IPatchRequestParser> _patchRequestParserStub;
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IApiResponseFactory> _apiResponseFactoryStub;
        private Mock<IFilterParser> _filterParserStub;
        private Mock<IJsonParser> _jsonParserStub;
        private Mock<IErrorResponseFactory> _errorResponseFactoryStub;
        private Mock<IRepresentationResponseParser> _responseParserStub;
        private Mock<IParametersValidator> _parametersValidatorStub;
        private IPatchRepresentationAction _patchRepresentationAction;

        #region Global

        [Fact]
        public void When_Passing_Null_Or_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute(null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute(string.Empty, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), null, null, null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), string.Empty, null, null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), "schemaid", "http://localhost:{id}", null));
            Assert.Throws<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), "schemaid", "http://localhost:{id}", string.Empty));
        }

        [Fact]
        public void When_Representation_Doesnt_Exist_Then_NotFound_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns((Representation)null);

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERTS
            _apiResponseFactoryStub.Verify(a => a.CreateError(HttpStatusCode.NotFound, string.Format(ErrorMessages.TheResourceDoesntExist, id)));
        }

        [Fact]
        public void When_Request_Is_Invalid_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation());
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns((IEnumerable<PatchOperation>)null);

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _apiResponseFactoryStub.Verify(a => a.CreateError((HttpStatusCode)errorResponse.Status, errorResponse));
        }

        [Fact]
        public void When_Passing_Remove_Operation_And_Path_Is_Not_Specified_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation());
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new []
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.ThePathNeedsToBeSpecified, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public void When_Passing_Invalid_Filter_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new []
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = true, Type = Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "members"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "not_valid_members"
                        }
                    }
                });
            
            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheFilterIsNotCorrect, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidFilter));
        }

        [Fact]
        public void When_Trying_To_Modify_Immutable_Attribute_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex, Mutability = Constants.SchemaAttributeMutability.Immutable })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "members"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, "members"), HttpStatusCode.BadRequest, Constants.ScimTypeValues.Mutability));
        }

        #endregion

        #region Remove

        [Fact]
        public void When_Trying_To_Remove_An_Element_From_A_Not_Array_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "members"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheRepresentationCannotBeRemovedBecauseItsNotAnArray, HttpStatusCode.BadRequest));
        }
        
        [Fact]
        public void When_Attribute_Cannot_Be_Removed_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = true, Type = "not_supported" })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "members"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheRepresentationCannotBeRemoved, HttpStatusCode.BadRequest));
        }

        #endregion

        #region Add

        [Fact]
        public void When_Trying_To_Add_An_Element_And_No_Value_Is_Passed_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.add,
                        Path = "members",
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>()))
                .Returns((RepresentationAttribute)null);

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheValueMustBeSpecified, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public void When_Trying_To_Add_An_Element_To_A_Not_Array_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.add,
                        Path = "members",
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>()))
                .Returns(new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex })
                {
                    Values = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry2")
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheRepresentationCannotBeAddedBecauseItsNotAnArray, HttpStatusCode.BadRequest));
        }

        #endregion
        
        #region Replace

        [Fact]
        public void When_Trying_To_Replace_An_Element_And_No_Value_Is_Passed_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.replace,
                        Path = "members",
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>()))
                .Returns((RepresentationAttribute)null);

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheValueMustBeSpecified, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public void When_Trying_To_Replace_An_Element_And_Something_Goes_Wrong_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = "invalid" })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                });
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.replace,
                        Path = "members",
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "members"
                        }
                    }
                });
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>()))
                .Returns(new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = "invalid" })
                {
                    Values = new[]
                    {
                        new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Constants.SchemaAttributeTypes.String }, "thierry2")
                    }
                });

            // ACT
            _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}", "resource_type");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheRepresentationCannotBeSet, HttpStatusCode.BadRequest));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _patchRequestParserStub = new Mock<IPatchRequestParser>();
            _representationStoreStub = new Mock<IRepresentationStore>();
            _apiResponseFactoryStub = new Mock<IApiResponseFactory>();
            _filterParserStub = new Mock<IFilterParser>();
            _jsonParserStub = new Mock<IJsonParser>();
            _errorResponseFactoryStub = new Mock<IErrorResponseFactory>();
            _responseParserStub = new Mock<IRepresentationResponseParser>();
            _parametersValidatorStub = new Mock<IParametersValidator>();
            _patchRepresentationAction = new PatchRepresentationAction(
                _patchRequestParserStub.Object,
               _representationStoreStub.Object,
               _apiResponseFactoryStub.Object,
               _filterParserStub.Object,
               _jsonParserStub.Object,
               _errorResponseFactoryStub.Object,
               _responseParserStub.Object,
               _parametersValidatorStub.Object);
        }
    }
}
