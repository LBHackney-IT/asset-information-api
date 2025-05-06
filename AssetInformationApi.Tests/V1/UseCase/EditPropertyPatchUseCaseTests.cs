using AssetInformationApi.V1.Factories;
using AssetInformationApi.V1.Gateways.Interfaces;
using AssetInformationApi.V1.Infrastructure;
using AssetInformationApi.V1.UseCase;
using AutoFixture;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Infrastructure;
using Moq;
using System.Threading.Tasks;
using System;
using Xunit;
using FluentAssertions;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]

    public class EditPropertyPatchUseCaseTests : EditAssetTestBase
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly EditPropertyPatchUseCase _classUnderTest;
        private readonly Mock<ISnsGateway> _assetSnsGateway;
        private readonly Mock<ISnsFactory> _assetSnsFactory;

        public EditPropertyPatchUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _assetSnsGateway = new Mock<ISnsGateway>();
            _assetSnsFactory = new Mock<ISnsFactory>();
            _classUnderTest = new EditPropertyPatchUseCase(_mockGateway.Object, _assetSnsGateway.Object, _assetSnsFactory.Object);
        }

        [Fact]
        public async Task EditPropertyPatchDetailsUseCaseWhenAssetDoesntExistReturnsNull()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditPropertyPatchRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                        .ReturnsAsync((UpdateEntityResult<AssetDb>) null);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeNull();
        }

        [Fact]
        public async Task EditPropertyPatchDetailsUseCaseWhenAssetExistsReturnsAssetResponseObject()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditPropertyPatchRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResponse = new UpdateEntityResult<AssetDb>
            {
                UpdatedEntity = _fixture.Create<AssetDb>()
            };

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                        .ReturnsAsync(gatewayResponse);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeOfType(typeof(AssetResponseObject));

            response.AssetAddress.Should().Be(gatewayResponse.UpdatedEntity.AssetAddress);
            response.Id.Should().Be(gatewayResponse.UpdatedEntity.Id);
        }

        [Fact]
        public async Task EditPropertyPatchDetailsUseCaseWhenNoChangesSNSGatewayIsntCalled()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditPropertyPatchRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereNoChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeOfType(typeof(AssetResponseObject));

            _assetSnsGateway.Verify(x => x.Publish(It.IsAny<EntityEventSns>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditPropertyPatchDetailsUseCaseWhenChangesSNSGatewayIsCalled()
        {
            // Arrange
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditPropertyPatchRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
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
