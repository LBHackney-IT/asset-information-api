using AssetInformationApi.V1.UseCase;
using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Boundary.Response;
using AssetInformationApi.V1.Gateways.Interfaces;
using Hackney.Core.DynamoDb.EntityUpdater;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class EditAssetAddressUseCaseTests : EditAssetTestBase
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly EditAssetAddressUseCase _classUnderTest;
        private readonly Mock<ISnsGateway> _assetSnsGateway;
        private readonly Mock<ISnsFactory> _assetSnsFactory;

        public EditAssetAddressUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _assetSnsGateway = new Mock<ISnsGateway>();
            _assetSnsFactory = new Mock<ISnsFactory>();
            _classUnderTest = new EditAssetAddressUseCase(_mockGateway.Object, _assetSnsGateway.Object, _assetSnsFactory.Object);
        }

        [Fact]
        public async Task EditAssetAddressDetailsUseCaseWhenAssetDoesntExistReturnsNull()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetAddressRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync((UpdateEntityResult<AssetDb>) null);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeNull();
        }

        [Fact]
        public async Task EditAssetAddressDetailsUseCaseWhenAssetExistsReturnsAssetResponseObject()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetAddressRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResponse = new UpdateEntityResult<AssetDb>
            {
                UpdatedEntity = _fixture.Create<AssetDb>()
            };

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(gatewayResponse);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeOfType(typeof(AssetResponseObject));

            response.AssetAddress.Should().Be(gatewayResponse.UpdatedEntity.AssetAddress);
            response.Id.Should().Be(gatewayResponse.UpdatedEntity.Id);
        }

        [Fact]
        public async Task EditAssetAddressDetailsUseCaseWhenNoChangesSNSGatewayIsntCalled()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetAddressRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereNoChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeOfType(typeof(AssetResponseObject));

            _assetSnsGateway.Verify(x => x.Publish(It.IsAny<EntityEventSns>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditAssetAddressDetailsUseCaseWhenChangesSNSGatewayIsCalled()
        {
            // Arrange
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetAddressRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var snsEvent = _fixture.Create<EntityEventSns>();

            _assetSnsFactory
             .Setup(x => x.UpdateAsset(gatewayResult, It.IsAny<Token>()))
             .Returns(snsEvent);

            // Act
            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(AssetResponseObject));
            _assetSnsGateway.Verify(x => x.Publish(snsEvent, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
