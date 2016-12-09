using Moq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Translation
{
    public sealed class TranslationManagerFixture
    {
        private Mock<IConfigurationService> _simpleIdentityServerConfiguratorFake;
        private Mock<ITranslationRepository> _translationRepositoryFake;
        private ITranslationManager _translationManager;

        [Fact]
        public async Task When_Passing_No_Translation_Codes_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _translationManager.GetTranslationsAsync(string.Empty, null));
        }

        [Fact]
        public async Task When_Passing_No_Preferred_Language_Then_Codes_Are_Translated_And_Default_Language_Is_Used()
        {
            // ARRANGE
            InitializeFakeObjects();
            var translationCodes = new List<string>
            {
                "translation_code"
            };
            var defaultLanguage = "EN";
            var translation = new Models.Translation
            {
                Code = "code",
                Value = "value"
            };
            _simpleIdentityServerConfiguratorFake.Setup(s => s.DefaultLanguageAsync())
                .Returns(Task.FromResult(defaultLanguage));
            _translationRepositoryFake.Setup(t => t.GetAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(translation)); ;

            // ACT
            var result = await _translationManager.GetTranslationsAsync(string.Empty, translationCodes);

            // ASSERT
            Assert.True(result.Count == 1);
            Assert.True(result.First().Key == translation.Code && result.First().Value == translation.Value);
        }

        [Fact]
        public async Task When_Passing_No_Preferred_Language_And_There_Is_No_Translation_Then_Codes_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var translationCodes = new List<string>
            {
                "translation_code"
            };
            var defaultLanguage = "EN";
            _simpleIdentityServerConfiguratorFake.Setup(s => s.DefaultLanguageAsync())
                .Returns(Task.FromResult(defaultLanguage));
            _translationRepositoryFake.Setup(t => t.GetAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult((Models.Translation)null)); 

            // ACT
            var result = await _translationManager.GetTranslationsAsync(string.Empty, translationCodes);

            // ASSERT
            Assert.True(result.Count == 1);
            Assert.True(result.First().Key == "translation_code" 
                && result.First().Value == "[translation_code]");
        }

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerConfiguratorFake = new Mock<IConfigurationService>();
            _translationRepositoryFake = new Mock<ITranslationRepository>();
            _translationManager = new TranslationManager(
                _simpleIdentityServerConfiguratorFake.Object,
                _translationRepositoryFake.Object);
        }
    }
}
