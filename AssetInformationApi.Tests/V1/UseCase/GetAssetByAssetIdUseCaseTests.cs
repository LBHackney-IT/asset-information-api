using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase;
using AutoFixture;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Xunit;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class GetAssetByAssetIdUseCaseTests
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly GetAssetByAssetIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public GetAssetByAssetIdUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _classUnderTest = new GetAssetByAssetIdUseCase(_mockGateway.Object);
        }


        [Fact]
        public async Task WhenResponseIsNullReturnsNull()
        {
            // Arrange
            var query = new GetAssetByAssetIdRequest
            {
                AssetId = _fixture.Create<string>()
            };

            Asset gatewayResponse = null;

            _mockGateway
                .Setup(x => x.GetAssetByAssetId(It.IsAny<GetAssetByAssetIdRequest>()))
                .ReturnsAsync(gatewayResponse);

            // Act
            var response = await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task WhenEntityReturnedReturnsResponseObject()
        {
            // Arrange
            Asset gatewayResponse = _fixture.Create<Asset>();

            _mockGateway
                .Setup(x => x.GetAssetByAssetId(It.IsAny<GetAssetByAssetIdRequest>()))
                .ReturnsAsync(gatewayResponse);

            var query = new GetAssetByAssetIdRequest
            {
                AssetId = gatewayResponse.AssetId
            };

            // Act
            var response = await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            response.Should().BeEquivalentTo(gatewayResponse.ToResponse());
        }
    }
}
