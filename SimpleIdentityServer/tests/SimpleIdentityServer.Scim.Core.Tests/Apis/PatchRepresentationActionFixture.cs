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
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
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
        private Mock<IRepresentationRequestParser> _representationRequestParserStub;
        private IPatchRepresentationAction _patchRepresentationAction;

        #region Global

        [Fact]
        public async Task When_Passing_Null_Or_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _patchRepresentationAction.Execute(null, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _patchRepresentationAction.Execute(string.Empty, null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _patchRepresentationAction.Execute("id", new JObject(), string.Empty, null));
        }

        [Fact]
        public async Task When_Representation_Doesnt_Exist_Then_NotFound_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult((Representation)null));

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERTS
            _apiResponseFactoryStub.Verify(a => a.CreateError(HttpStatusCode.NotFound, string.Format(ErrorMessages.TheResourceDoesntExist, id)));
        }

        [Fact]
        public async Task When_Request_Is_Invalid_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation()));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns((IEnumerable<PatchOperation>)null);

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _apiResponseFactoryStub.Verify(a => a.CreateError((HttpStatusCode)errorResponse.Status, errorResponse));
        }

        [Fact]
        public async Task When_Passing_Remove_Operation_And_Path_Is_Not_Specified_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation()));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new []
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove
                    }
                });

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.ThePathNeedsToBeSpecified, HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public async Task When_Passing_Invalid_Filter_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new []
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = true, Type = Common.Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
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
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheFilterIsNotCorrect, HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.InvalidFilter));
        }

        [Fact]
        public async Task When_Trying_To_Modify_Immutable_Attribute_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex, Mutability = Common.Constants.SchemaAttributeMutability.Immutable })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
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
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, "members"), HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.Mutability));
        }

        [Fact]
        public async Task When_Trying_To_Set_A_Unique_Attribute_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex, Mutability = Common.Constants.SchemaAttributeMutability.ReadWrite, Uniqueness = Common.Constants.SchemaAttributeUniqueness.Server })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
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
            _representationStoreStub.Setup(r => r.SearchValues(It.IsAny<string>(), It.IsAny<Filter>()))
                .Returns(Task.FromResult((IEnumerable<RepresentationAttribute>)new List<RepresentationAttribute>
                {
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex, Mutability = Common.Constants.SchemaAttributeMutability.ReadWrite, Uniqueness = Common.Constants.SchemaAttributeUniqueness.Server })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                                }
                            }
                }));

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(string.Format(ErrorMessages.TheAttributeMustBeUnique, "members"), HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.Uniqueness));
        }

        #endregion

        #region Remove

        [Fact]
        public async Task When_Trying_To_Remove_Singular_Attribute_Then_It_Is_Removed_From_The_Representation()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            Representation result = null;
            var representation = new Representation();
            representation.Attributes = new[]
            {
                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
            };
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(representation));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "firstName"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "firstName"
                        }
                    }
                });
            _responseParserStub.Setup(p => p.Parse(It.IsAny<Representation>(), It.IsAny<string>(), It.IsAny<string>(), OperationTypes.Modification))
                .Callback((Representation repr, string loc, string schem, OperationTypes op) =>
                {
                    result = repr;
                })
                .Returns(Task.FromResult(new Response
                {
                    Location = "location",
                    Object = new JObject()
                }));

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            Assert.NotNull(result);
            Assert.False(result.Attributes.Any());
        }

        [Fact]
        public async Task When_Trying_To_Remove_Person_FirstName_Then_It_Is_Removed_From_The_Representation()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            Representation result = null;
            var representation = new Representation();
            var person = new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "person", Type = Common.Constants.SchemaAttributeTypes.Complex });
            person.Values = new[]
            {
                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                {
                    Parent = person
                }
            };
            representation.Attributes = new[] { person };
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(representation));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.remove,
                        Path = "person.firstName"
                    }
                });
            _filterParserStub.Setup(f => f.Parse(It.IsAny<string>()))
                .Returns(new Filter
                {
                    Expression = new AttributeExpression
                    {
                        Path = new AttributePath
                        {
                            Name = "person",
                            Next = new AttributePath
                            {
                                    Name = "firstName"
                            }
                        }
                    }
                });
            _responseParserStub.Setup(p => p.Parse(It.IsAny<Representation>(), It.IsAny<string>(), It.IsAny<string>(), OperationTypes.Modification))
                .Callback((Representation repr, string loc, string schem, OperationTypes op) =>
                {
                    result = repr;
                })
                .Returns(Task.FromResult(new Response
                {
                    Location = "location",
                    Object = new JObject()
                }));

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Attributes.Count() == 1);
            var complex = result.Attributes.First() as ComplexRepresentationAttribute;
            Assert.NotNull(complex);
            Assert.False(complex.Values.Any());
        }

        [Fact]
        public async Task When_Attribute_Cannot_Be_Removed_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest
            };
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = true, Type = "not_supported" })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
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
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheRepresentationCannotBeRemoved, HttpStatusCode.BadRequest));
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Sending_Add_Operation_But_No_Value_Is_Specified_In_Parameter_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.add,
                        Path = "members",
                        Value = null
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
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>(), CheckStrategies.Standard))
                .Returns((ParseRepresentationAttrResult)null);

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheValueNeedsToBeSpecified, HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public async Task When_Sending_Add_Operation_And_Path_Parameter_Is_Not_Specified_Then_Values_Are_Added_To_The_Representation()
        {
            // ARRANGE
            const string id = "id";
            var representation = new Representation
            {
                Attributes = new RepresentationAttribute[0]
            };
            Representation result = null;
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(s => s.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(representation));
            _representationRequestParserStub.Setup(r => r.Parse(It.IsAny<JToken>(), It.IsAny<string>(), CheckStrategies.Standard))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = true,
                    Representation = new Representation
                    {
                        Attributes = new[]
                        {
                            new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex })
                            {
                                Values = new []
                                {
                                    new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "firstName")
                                }
                            }
                        }
                    }
                }));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.add,
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _responseParserStub.Setup(p => p.Parse(It.IsAny<Representation>(), It.IsAny<string>(), It.IsAny<string>(), OperationTypes.Modification))
                .Callback((Representation repr, string loc, string schem, OperationTypes op) =>
                {
                    result = repr;
                })
                .Returns(Task.FromResult(new Response
                {
                    Location = "location",
                    Object = new JObject()
                }));
                

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Attributes.Any());
        }

        [Fact]
        public async Task When_Sending_Add_Operation_And_Path_Parameter_Is_Not_Passed_And_Value_Cannot_Be_Parsed_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            var representation = new Representation
            {
                Attributes = new RepresentationAttribute[0]
            };
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.add,
                        Value = JObject.Parse("{ members : [ { firstName : 'firstName' }] }")
                    }
                });
            _representationStoreStub.Setup(s => s.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(representation));
            _representationRequestParserStub.Setup(r => r.Parse(It.IsAny<JToken>(), It.IsAny<string>(), CheckStrategies.Standard))
                .Returns(Task.FromResult(new ParseRepresentationResult
                {
                    IsParsed = false
                }));


            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _apiResponseFactoryStub.Verify(a => a.CreateError(HttpStatusCode.BadRequest, It.IsAny<ErrorResponse>()));
        }

        #endregion
        
        #region Replace

        [Fact]
        public async Task When_Sending_Replace_Operation_But_No_Value_Is_Passed_In_Parameter_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = Common.Constants.SchemaAttributeTypes.Complex })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
            _patchRequestParserStub.Setup(p => p.Parse(It.IsAny<JObject>(), out errorResponse))
                .Returns(new[]
                {
                    new PatchOperation
                    {
                        Type = PatchOperations.replace,
                        Path = "members",
                        Value = null
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
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>(), CheckStrategies.Standard))
                .Returns(new ParseRepresentationAttrResult
                {
                    IsParsed = false
                });

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

            // ASSERT
            _errorResponseFactoryStub.Verify(a => a.CreateError(ErrorMessages.TheValueNeedsToBeSpecified, HttpStatusCode.BadRequest, Common.Constants.ScimTypeValues.InvalidSyntax));
        }

        [Fact]
        public async Task When_Trying_To_Replace_An_Element_And_Something_Goes_Wrong_Then_BadRequest_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            ErrorResponse errorResponse = new ErrorResponse();
            InitializeFakeObjects();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation
                {
                    Attributes = new[]
                    {
                        new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = "invalid" })
                        {
                            Values = new []
                            {
                                new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry")
                            }
                        }
                    }
                }));
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
            _jsonParserStub.Setup(j => j.GetRepresentation(It.IsAny<JToken>(), It.IsAny<SchemaAttributeResponse>(), CheckStrategies.Standard))
                .Returns(new ParseRepresentationAttrResult
                {
                    IsParsed = true,
                    RepresentationAttribute = new ComplexRepresentationAttribute(new SchemaAttributeResponse { Name = "members", MultiValued = false, Type = "invalid" })
                    {
                        Values = new[]
                        {
                            new SingularRepresentationAttribute<string>(new SchemaAttributeResponse { Name = "firstName", Type = Common.Constants.SchemaAttributeTypes.String }, "thierry2")
                        }
                    }
                });

            // ACT
            await _patchRepresentationAction.Execute(id, new JObject(), "schema_id", "http://localhost:{id}");

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
            _representationRequestParserStub = new Mock<IRepresentationRequestParser>();
            _patchRepresentationAction = new PatchRepresentationAction(
                _patchRequestParserStub.Object,
               _representationStoreStub.Object,
               _apiResponseFactoryStub.Object,
               _filterParserStub.Object,
               _jsonParserStub.Object,
               _errorResponseFactoryStub.Object,
               _responseParserStub.Object,
               _parametersValidatorStub.Object,
               _representationRequestParserStub.Object);
        }
    }
}
