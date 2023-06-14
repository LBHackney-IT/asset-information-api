using AssetInformationApi.V1.Gateways;
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
using Hackney.Core.Http;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Infrastructure;
using AssetInformationApi.V1.Infrastructure;
using AssetInformationApi.V1.Boundary.Request;
using System.Collections.Generic;
using Hackney.Shared.Asset.Boundary.Response;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class EditAssetUseCaseTests : EditAssetTestBase
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly EditAssetUseCase _classUnderTest;
        private readonly Mock<ISnsGateway> _assetSnsGateway;
        private readonly Mock<ISnsFactory> _assetSnsFactory;

        public EditAssetUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _assetSnsGateway = new Mock<ISnsGateway>();
            _assetSnsFactory = new Mock<ISnsFactory>();
            _classUnderTest = new EditAssetUseCase(_mockGateway.Object, _assetSnsGateway.Object, _assetSnsFactory.Object);
        }

        [Fact]
        public async Task EditAssetDetailsUseCaseWhenAssetDoesntExistReturnsNull()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync((UpdateEntityResult<AssetDb>) null);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeNull();
        }

        [Fact]
        public async Task EditAssetDetailsUseCaseWhenAssetExistsReturnsAssetResponseObject()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResponse = new UpdateEntityResult<AssetDb>
            {
                UpdatedEntity = _fixture.Create<AssetDb>()
            };

            var expectedAssetCharacteristics = gatewayResponse.UpdatedEntity.AssetCharacteristics.ToDomain().ToResponse();

            _mockGateway.Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(gatewayResponse);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeOfType(typeof(AssetResponseObject));

            response.AssetCharacteristics.Should().BeEquivalentTo(expectedAssetCharacteristics);
            response.AssetManagement.Should().Be(gatewayResponse.UpdatedEntity.AssetManagement);
            response.Id.Should().Be(gatewayResponse.UpdatedEntity.Id);
        }

        [Fact]
        public async Task EditAssetDetailsUseCaseWhenNoChangesSNSGatewayIsntCalled()
        {
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            // setup mock gateway to return UpdateEntityResult with no changes
            var gatewayResult = MockUpdateEntityResultWhereNoChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            // assert result is AssetResponseObject
            response.Should().BeOfType(typeof(AssetResponseObject));

            // assert that sns factory wasnt called
            _assetSnsGateway.Verify(x => x.Publish(It.IsAny<EntityEventSns>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditAssetDetailsUseCaseWhenChangesSNSGatewayIsCalled()
        {
            // Arrange
            var mockQuery = new Guid();
            var mockRequestObject = _fixture.Create<EditAssetRequest>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereChangesAreMade();

            _mockGateway
                .Setup(x => x.EditAssetDetails(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<int?>()))
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
