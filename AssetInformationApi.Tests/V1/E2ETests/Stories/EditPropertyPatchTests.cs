using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using AssetInformationApi.Tests.V1.E2ETests.Steps;
using AutoFixture;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Domain;
using System;
using TestStack.BDDfy;
using Xunit;

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to edit an patch details",
        SoThat = "it is possible to edit the patch of an asset.")]
    [Collection("AppTest collection")]
    public class EditPropertyPatchTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly AssetsFixture _assetFixture;
        private readonly EditAssetSteps _steps;
        private readonly Fixture _fixture = new Fixture();

        public EditPropertyPatchTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _assetFixture = new AssetsFixture(_dbFixture, _snsFixture.SimpleNotificationService);
            _steps = new EditAssetSteps(appFactory.Client, _dbFixture.DynamoDbContext);

            Environment.SetEnvironmentVariable("PATCHES_ADMIN_GROUPS", "e2e-testing");
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
        public void EditPropertyPatchServiceReturns204()
        {
            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .Then(t => _assetFixture.CreateEditPropertyPatchObject())
                .When(w => _steps.WhenEditPropertyPatchApiIsCalled(_assetFixture.AssetId, _assetFixture.EditPropertyPatch))
                .Then(t => _steps.ThenNoContentResponseReturned())
                .And(a => _steps.TheAssetHasBeenUpdatedInTheDatabase(_assetFixture, true))
                .And(t => _steps.ThenTheAssetAddressOrPropertyPatchUpdatedEventIsRaised(_assetFixture, _snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns400BadRequest()
        {
            var invalidRequestObject = "bad-data";

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditPropertyPatchApiIsCalled(_assetFixture.AssetId, invalidRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturns400BadRequestForFailedValidation()
        {
            var requestWithEmptyPatchId = new EditPropertyPatchRequest
            {
                PatchId = Guid.Empty,
            };

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditPropertyPatchApiIsCalled(_assetFixture.AssetId, requestWithEmptyPatchId))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsNotFoundResponse()
        {
            var randomId = Guid.NewGuid();
            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenEditPropertyPatchApiIsCalled(randomId, requestObject))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();

        }


        [Fact]
        public void ServiceReturnsUnauthorizedWhenUserIsNotInAllowedGroups()
        {
            Environment.SetEnvironmentVariable("PATCHES_ADMIN_GROUPS", "unauthorized-group");

            var randomId = Guid.NewGuid();
            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditPropertyPatchApiIsCalled(randomId, requestObject))
                .Then(t => _steps.ThenUnauthorizedIsReturned())
                .BDDfy();
        }

        private EditPropertyPatchRequest CreateValidRequestObject()
        {
            return _fixture.Build<EditPropertyPatchRequest>()
                .Create();
        }

    }
}
