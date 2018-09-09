using Moq;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Core.Tests.Apis
{
    public class AddRepresentationActionFixture
    {
        private Mock<IRepresentationRequestParser> _representationRequestParserStub;
        private Mock<IRepresentationStore> _representationStoreStub;
        private Mock<IRepresentationResponseParser> _representationResponseParserStub;
        private Mock<IApiResponseFactory> _apiResponseFactoryStub;
        private Mock<IParametersValidator> _parametersValidatorStub;
        private IAddRepresentationAction _addRepresentationAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            Initialize();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(null, string.Empty, string.Empty, string.Empty, string.Empty));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, string.Empty, string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, null, string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, "schemaid", string.Empty, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, "schemaid", null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, "schemaid", "resourcetype", string.Empty));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addRepresentationAction.Execute(new JObject(), string.Empty, "schemaid", "resourcetype", null));
        }

        [Fact]
        public async Task When_Resource_Doesnt_Exist_Then_ErrorIsReturned()
        {
            // ARRANGE
            const string id = "id";
            Initialize();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation()));

            // ACT
            await _addRepresentationAction.Execute(new JObject(), string.Empty, "schema_id", "resource_type", id).ConfigureAwait(false);

            // ASSERT
            _apiResponseFactoryStub.Verify(a => a.CreateError(HttpStatusCode.InternalServerError, string.Format(ErrorMessages.TheResourceAlreadyExist, id)));
        }

        [Fact]
        public async Task When_Request_Cannot_Be_Parsed_Then_Error_Is_Returned()
        {
            // ARRANGE
            const string id = "id";
            string error;
            Initialize();
            _representationStoreStub.Setup(r => r.GetRepresentation(It.IsAny<string>()))
                .Returns(Task.FromResult(new Representation()));
            _representationRequestParserStub.Setup(r => r.Parse(It.IsAny<JObject>(), It.IsAny<string>(), CheckStrategies.Strong))
                .Returns(Task.FromResult((ParseRepresentationResult)null));

            // ACT
            await _addRepresentationAction.Execute(new JObject(), string.Empty, "schema_id", "resource_type", id).ConfigureAwait(false);

            // ASSERT
            _apiResponseFactoryStub.Verify(a => a.CreateError(HttpStatusCode.InternalServerError, It.IsAny<string>()));
        }

        private void Initialize()
        {
            _representationRequestParserStub = new Mock<IRepresentationRequestParser>();
            _representationStoreStub = new Mock<IRepresentationStore>();
            _representationResponseParserStub = new Mock<IRepresentationResponseParser>();
            _apiResponseFactoryStub = new Mock<IApiResponseFactory>();
            _parametersValidatorStub = new Mock<IParametersValidator>();
            _addRepresentationAction = new AddRepresentationAction(
                _representationRequestParserStub.Object,
                _representationStoreStub.Object,
                _representationResponseParserStub.Object,
                _apiResponseFactoryStub.Object,
                _parametersValidatorStub.Object);
        }
    }
}
