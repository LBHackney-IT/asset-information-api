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

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class NewAssetUseCaseTests
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly NewAssetUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public NewAssetUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _classUnderTest = new NewAssetUseCase(_mockGateway.Object);
        }

        [Fact]
        public async Task AddAssetUsecaseShouldReturnOkResponse()
        {
            var asset = _fixture.Create<Asset>();
            asset.Id = Guid.NewGuid();

            var request = asset.ToDatabase();
            _mockGateway.Setup(x => x.AddAsset(request)).ReturnsAsync(asset);

            var response = await _classUnderTest.PostAsync(request).ConfigureAwait(false);
            response.Should().BeEquivalentTo(asset.ToResponse());
        }

        [Fact]
        public async Task AddAssetUsecaseShouldReturnNull()
        {
            var asset = _fixture.Create<Asset>();

            var request = asset.ToDatabase();
            _mockGateway.Setup(x => x.AddAsset(request)).ReturnsAsync(asset);

            var response = await _classUnderTest.PostAsync(asset.ToDatabase()).ConfigureAwait(false);
            response.Should().BeNull();
        }
    }
}
