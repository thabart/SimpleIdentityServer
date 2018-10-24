using Moq;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class AmrHelperFixture
    {
        private IAmrHelper _amrHelper;

        [Fact]
        public void When_No_Amr_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _amrHelper.GetAmr(new List<string>(), new[] { "pwd" }));
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.NoActiveAmr, exception.Message);
        }

        [Fact]
        public void When_Amr_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _amrHelper.GetAmr(new List<string> { "invalid" }, new[] { "pwd" }));
            Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(Errors.ErrorDescriptions.TheAmrDoesntExist, "pwd"), exception.Message);
        }

        [Fact]
        public void When_Amr_Doesnt_Exist_Then_Default_One_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var amr = _amrHelper.GetAmr(new List<string> { "pwd" }, new[] { "invalid" });

            // ASSERTS
            Assert.Equal("pwd", amr);
        }

        [Fact]
        public void When_Amr_Exists_Then_Same_Amr_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var amr = _amrHelper.GetAmr(new List<string> { "amr" }, new[] { "amr" });

            // ASSERTS
            Assert.Equal("amr", amr);
        }

        private void InitializeFakeObjects()
        {
            _amrHelper = new AmrHelper();
        }
    }
}
