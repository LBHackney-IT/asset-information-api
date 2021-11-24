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
    [Collection("AppTest collection")]
    public class GetAssetByAssetIdTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly GetAssetByAssetIdSteps _steps;
        private readonly AssetsFixture _assetsFixture;

        public GetAssetByAssetIdTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _assetsFixture = new AssetsFixture(_dbFixture);
            _steps = new GetAssetByAssetIdSteps(appFactory.Client);
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
        public void ServiceReturnsNotFoundWhenEntityDoesntExist()
        {
            this.Given(g => _assetsFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenTheGetApiIsCalled(_assetsFixture.PropertyReference))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsEntityWhenExists()
        {
            this.Given(g => _assetsFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenTheGetApiIsCalled(_assetsFixture.PropertyReference))
                .Then(t => _steps.ThenTheAssetDetailsAreReturned(_assetsFixture.Asset))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWhenAssetIdContainsTags()
        {
            var stringWithTags = "Some string with <tag> in it.";

            this.Given(g => _assetsFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenTheGetApiIsCalled(stringWithTags))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }
    }
}
