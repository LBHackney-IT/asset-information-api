using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase;
using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Moq;
using System;
using System.Threading.Tasks;
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
        public async Task WhenCalledCallsGateway()
        {
            // Arrange
            var query = new GetAssetByAssetIdRequest
            {
                AssetId = _fixture.Create<string>()
            };

            // Act
            await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            _mockGateway.Verify(x => x.GetAssetByAssetId(query), Times.Once);
        }
    }
}
