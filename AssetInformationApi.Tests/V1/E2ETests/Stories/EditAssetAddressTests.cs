using AutoFixture;
using Hackney.Core.Testing.Sns;
using System;
using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using AssetInformationApi.Tests.V1.E2ETests.Steps;
using TestStack.BDDfy;
using Xunit;
using Hackney.Core.Testing.DynamoDb;
using AssetInformationApi.V1.Boundary.Request;
using AutoFixture.Kernel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Boundary.Request;

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to edit an existing address",
        SoThat = "it is possible to edit the address of an asset.")]
    [Collection("AppTest collection")]
    public class EditAssetAddressTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly AssetsFixture _assetFixture;
        private readonly EditAssetSteps _steps;
        private readonly Fixture _fixture = new Fixture();

        public EditAssetAddressTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _assetFixture = new AssetsFixture(_dbFixture, _snsFixture.SimpleNotificationService);
            _steps = new EditAssetSteps(appFactory.Client, _dbFixture.DynamoDbContext);
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
                _assetFixture?.Dispose();
                _snsFixture?.PurgeAllQueueMessages();

                _disposed = true;
            }
        }

        [Fact]
        public void AddressEditServiceReturns204AndUpdatesDatabase()
        {
            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .Then(t => _assetFixture.CreateEditAssetAddressObject())
                .When(w => _steps.WhenEditAssetAddressApiIsCalled(_assetFixture.AssetId, _assetFixture.EditAssetAddress))
                .Then(t => _steps.ThenNoContentResponseReturned())
                .And(a => _steps.TheAssetHasBeenUpdatedInTheDatabase(_assetFixture))
                .And(t => _steps.ThenTheAssetAddressUpdatedEventIsRaised(_assetFixture, _snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns400BadRequest()
        {
            var invalidRequestObject = "bad-data";

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditAssetAddressApiIsCalled(_assetFixture.AssetId, invalidRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsNotFoundResponse()
        {
            var randomId = Guid.NewGuid();
            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenEditAssetAddressApiIsCalled(randomId, requestObject))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();

        }

        private EditAssetAddressRequest CreateValidRequestObject()
        {
            return _fixture.Build<EditAssetAddressRequest>()
                .Create();
        }

    }
}
