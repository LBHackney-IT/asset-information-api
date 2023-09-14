using AssetInformationApi.V1.UseCase;
using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Xunit;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using Microsoft.Extensions.Logging.Abstractions;
using AssetInformationApi.V1.Gateways.Interfaces;
using AssetInformationApi.V1.Boundary.Request;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Boundary.Response;
using AssetInformationApi.V1.Infrastructure;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class NewAssetUseCaseTests
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly NewAssetUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ISnsGateway> _assetSnsGateway;
        private readonly Mock<ISnsFactory> _assetSnsFactory;

        public NewAssetUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _assetSnsGateway = new Mock<ISnsGateway>();
            _assetSnsFactory = new Mock<ISnsFactory>();
            _classUnderTest = new NewAssetUseCase(_mockGateway.Object, _assetSnsGateway.Object, _assetSnsFactory.Object, new NullLogger<NewAssetUseCase>());
        }

        [Fact]
        public async Task AddAssetUsecaseShouldReturnOkResponse()
        {
            // ARRANGE
            // Valid inbound request
            AddAssetRequest newAssetRequest = _fixture.Create<AddAssetRequest>();

            // Convert AddAssetRequest into AssetDb object (used by gateway method)
            AssetDb assetDb = newAssetRequest.ToDatabase();

            // Convert AssetDb into Asset object (used for response comparison)
            Asset asset = assetDb.ToDomain();

            var token = new Token();

            _mockGateway.Setup(x => x.AddAsset(It.IsAny<AssetDb>())).ReturnsAsync(asset);

            // ACT
            var response = await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);

            // ASSERT
            response.Should().BeEquivalentTo(asset.ToResponse());
        }


        [Fact]
        public async Task AddAssetUsecaseShouldReturnNullWhenAssetIsInvalid()
        {
            // ARRANGE
            // Invalid inbound request (no Id)
            AddAssetRequest newAssetRequest = _fixture.Build<AddAssetRequest>().Without(requestFixture => requestFixture.Id).Create();

            // Convert AddAssetRequest into AssetDb object (used by gateway method)
            AssetDb assetDb = newAssetRequest.ToDatabase();

            // Convert AssetDb into Asset object (used for response comparison)
            Asset asset = assetDb.ToDomain();

            var token = new Token();
            token.Email = "test@test.com";
            token.Name = "Test";

            // No asset Id will result in null gateway response 
            _mockGateway.Setup(x => x.AddAsset(newAssetRequest.ToDatabase())).ReturnsAsync(asset);

            // ACT
            var response = await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);

            // ASSERT
            response.Should().BeNull();
        }

        [Fact]
        public async Task AddAssetUsecaseShouldPublishSnsMessageWhenAddDefaultSorContractsIsTrue()
        {
            // ARRANGE
            AddAssetRequest newAssetRequest = _fixture.Create<AddAssetRequest>();
            AssetDb assetDb = newAssetRequest.ToDatabase();
            Asset asset = assetDb.ToDomain();
            var token = new Token();

            // This will trigger the publishing of the AddRepairsContract event
            newAssetRequest.AddDefaultSorContracts = true;

            _mockGateway.Setup(x => x.AddAsset(It.IsAny<AssetDb>())).ReturnsAsync(asset);

            var assetContractsSnsMessage = new EntityEventSns();

            _assetSnsFactory.Setup(x => x.AddRepairsContractsToNewAsset(It.IsAny<AddRepairsContractsToNewAssetObject>(), It.IsAny<Token>()))
                            .Returns(assetContractsSnsMessage);

            // ACT
            await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);

            // ASSERT
            _assetSnsFactory.Verify(x => x.AddRepairsContractsToNewAsset(It.IsAny<AddRepairsContractsToNewAssetObject>(), It.IsAny<Token>()), Times.Once);
        }

        [Fact]
        public async Task AddAssetUsecaseShouldNotPublishSnsMessageWhenAddDefaultSorContractsIsFalse()
        {
            // ARRANGE
            AddAssetRequest newAssetRequest = _fixture.Create<AddAssetRequest>();
            AssetDb assetDb = newAssetRequest.ToDatabase();
            Asset asset = assetDb.ToDomain();
            var token = new Token();

            // This will NOT trigger the publishing of the AddRepairsContract event
            newAssetRequest.AddDefaultSorContracts = false;

            _mockGateway.Setup(x => x.AddAsset(It.IsAny<AssetDb>())).ReturnsAsync(asset);

            var assetContractsSnsMessage = new EntityEventSns();

            _assetSnsFactory.Setup(x => x.AddRepairsContractsToNewAsset(It.IsAny<AddRepairsContractsToNewAssetObject>(), It.IsAny<Token>()))
                            .Returns(assetContractsSnsMessage);

            // ACT
            await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);

            // ASSERT
            _assetSnsFactory.Verify(x => x.AddRepairsContractsToNewAsset(It.IsAny<AddRepairsContractsToNewAssetObject>(), It.IsAny<Token>()), Times.Never);
        }
    }
}
