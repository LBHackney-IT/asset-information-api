using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using AssetInformationApi.Tests.V1.E2ETests.Steps;
using AssetInformationApi.V1.Boundary.Request;
using AutoFixture;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using Hackney.Shared.Asset.Boundary.Request;
using System;
using TestStack.BDDfy;
using Xunit;

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to add a new asset and return asset details",
        SoThat = "it is possible to view the details of an asset.")]
    [Collection("AppTest collection")]
    public class AddNewAssetTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly AddNewAssetSteps _steps;
        private readonly AssetsFixture _assetsFixture;

        //private readonly Fixture _fixture = new Fixture();

#pragma warning disable 0649
        public AddNewAssetTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _assetsFixture = new AssetsFixture(_dbFixture, _snsFixture.SimpleNotificationService);
            _steps = new AddNewAssetSteps(appFactory.Client, _dbFixture);

            Environment.SetEnvironmentVariable("ASSET_ADMIN_GROUPS", "e2e-testing");
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

                _snsFixture?.PurgeAllQueueMessages();

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedAsset()
        {
            this.Given(g => _assetsFixture.PrepareAssetObject())
                .When(w => _steps.WhenTheAddAssetApiIsCalledWithAToken(_assetsFixture.AssetRequest))
                .Then(t => _steps.ThenTheAssetDetailsAreReturned(_assetsFixture.AssetRequest))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestIfIdInvalid()
        {
            this.Given(g => _assetsFixture.GivenAnEmptyAssetId())
                .When(w => _steps.WhenTheAddAssetApiIsCalled(_assetsFixture.AssetRequest))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceAddsNewAssetAndPostsToSns()
        {
            this.Given(g => _assetsFixture.PrepareAssetObject())
                .When(w => _steps.WhenTheAddAssetApiIsCalledWithAToken(_assetsFixture.AssetRequest))
                .Then(t => _steps.ThenAssetDetailsAreReturnedAndTheAssetCreatedEventIsRaised(_assetsFixture.AssetRequest, _snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsUnauthorizedWhenUserIsNotInAllowedGroups()
        {
            Environment.SetEnvironmentVariable("ASSET_ADMIN_GROUPS", "unauthorized-group");

            this.Given(g => _assetsFixture.PrepareAssetObject())
                .When(w => _steps.WhenTheAddAssetApiIsCalled(_assetsFixture.AssetRequest))
                .Then(t => _steps.ThenUnauthorizedIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns500IfAssetIdDuplicated()
        {
            this.Given(g => _assetsFixture.PrepareAssetObjectWithAssetId())
                .When(w => _steps.WhenTheAddAssetApiIsCalledWithAToken(_assetsFixture.AssetRequest))
                .Given(g => _assetsFixture.PrepareAssetObjectWithAssetId())
                .When(w => _steps.WhenTheAddAssetApiIsCalledWithAToken(_assetsFixture.AssetRequest))
                .Then(t => _steps.Then500IsReturned())
                .BDDfy();
        }
    }
}
