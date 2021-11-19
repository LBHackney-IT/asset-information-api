using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using AssetInformationApi.Tests.V1.E2ETests.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to return asset details",
        SoThat = "it is possible to view the details of an asset.")]
    [Collection("DynamoDb collection")]
    public class GetAssetByAssetIdTests : IDisposable
    {
        private readonly DynamoDbIntegrationTests<Startup> _dbFixture;
        private readonly GetAssetByAssetIdSteps _steps;
        private readonly AssetsFixture _assetsFixture;

        public GetAssetByAssetIdTests(DynamoDbIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _assetsFixture = new AssetsFixture(_dbFixture.DynamoDbContext);
            _steps = new GetAssetByAssetIdSteps(_dbFixture.Client);
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
    }
}
