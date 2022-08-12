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

namespace AssetInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Service",
        IWant = "an endpoint to edit an existing asset and return a 204 response",
        SoThat = "it is possible to edit the details of an asset.")]
    [Collection("AppTest collection")]
    public class EditAssetTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly AssetsFixture _assetFixture;
        private readonly EditAssetSteps _steps;
        private readonly Fixture _fixture = new Fixture();

        public EditAssetTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _assetFixture = new AssetsFixture(_dbFixture, _snsFixture.SimpleNotificationService);
            _steps = new EditAssetSteps(appFactory.Client, _dbFixture.DynamoDbContext);
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IDynamoDbFixture),
                    typeof(DynamoDbFixture)));
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IDynamoDBContext),
                    typeof(DynamoDBContext)));
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IAmazonDynamoDB),
                    typeof(AmazonDynamoDBClient)));
            _fixture.Customizations.Add(
             new TypeRelay(
                 typeof(IAmazonSimpleNotificationService),
                 typeof(AmazonSimpleNotificationServiceClient)));
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
        public void ServiceReturns400BadRequest()
        {
            var invalidRequestObject = "bad-data";

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditAssetApiIsCalled(_assetFixture.AssetId, invalidRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsNotFoundResponse()
        {
            var randomId = Guid.NewGuid();
            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetThatDoesntExist())
                .When(w => _steps.WhenEditAssetApiIsCalled(randomId, requestObject))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();

        }

        [Fact]
        public void ServiceReturns204AndUpdatesDatabase()
        {
            // request with empty body should still return 204 no content.
            // the request also shouldnt update the database since no changes were sent.

            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditAssetApiIsCalled(_assetFixture.AssetId, requestObject))
                .Then(t => _steps.ThenNoContentResponseReturned())
                .And(a => _steps.TheAssetHasBeenUpdatedInTheDatabase(_assetFixture, requestObject))
                .And(t => _steps.ThenTheAssetUpdatedEventIsRaised(_assetFixture, _snsFixture))
                .BDDfy();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(5)]
        public void ServiceReturnsConflictWhenIncorrectVersionNumber(int? versionNumber)
        {
            var requestObject = CreateValidRequestObject();

            this.Given(g => _assetFixture.GivenAnAssetAlreadyExists())
                .When(w => _steps.WhenEditAssetApiIsCalled(_assetFixture.AssetId, requestObject, versionNumber))
                .Then(t => _steps.ThenConflictIsReturned(versionNumber))
                .BDDfy();
        }

        private EditAssetRequest CreateValidRequestObject()
        {
            return _fixture.Build<EditAssetRequest>()
                .Without(x => x.AssetAddress)
                .Without(x => x.AssetCharacteristics)
                .Without(x => x.AssetLocation)
                .Without(x => x.AssetManagement)
                .Without(x => x.Tenure)
                .With(x => x.VersionNumber, (int?) null)
                .Create();
        }
    }
}