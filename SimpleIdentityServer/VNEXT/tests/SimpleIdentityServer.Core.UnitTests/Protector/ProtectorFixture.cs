using Moq;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Core.UnitTests.Protector
{
    public sealed class ProtectorFixture
    {
        private Mock<ICertificateStore> _certificateStoreStub;

        private Mock<ICompressor> _compressorStub;

        private IProtector _protector;

        
        private void InitializeFakeObjects()
        {
            _certificateStoreStub = new Mock<ICertificateStore>();
            _compressorStub = new Mock<ICompressor>();
            _protector = new Core.Protector.Protector(
                _certificateStoreStub.Object,
                _compressorStub.Object);
        }
    }
}
