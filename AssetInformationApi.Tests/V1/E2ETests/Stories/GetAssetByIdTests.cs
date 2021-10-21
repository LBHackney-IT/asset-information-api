using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using AssetInformationApi.Tests.V1.E2ETests.Steps;
using Hackney.Core.Testing.DynamoDb;
using System;
using TestStack.BDDfy;
using Xunit;

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to return asset details",
        SoThat = "it is possible to view the details of an asset.")]
    [Collection("DynamoDb collection")]
    public class GetAssetByIdTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly GetAssetByIdSteps _steps;
        private readonly AssetsFixture _assetsFixture;

        public GetAssetByIdTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _assetsFixture = new AssetsFixture(_dbFixture);
            _steps = new GetAssetByIdSteps(appFactory.Client);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _assetsFixture)
                    _assetsFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedAsset()
        {
            this.Given(g => _assetsFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenTheGetApiIsCalled(_assetsFixture.AssetId.ToString()))
                .Then(t => _steps.ThenTheAssetDetailsAreReturned(_assetsFixture.Asset))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsNotFoundIfAssetNotExist()
        {
            this.Given(g => _assetsFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenTheGetApiIsCalled(_assetsFixture.AssetId.ToString()))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestIfIdInvalid()
        {
            this.Given(g => _assetsFixture.GivenAnInvalidAssetId())
                .When(w => _steps.WhenTheGetApiIsCalled(_assetsFixture.InvalidAssetId))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }
    }
}
