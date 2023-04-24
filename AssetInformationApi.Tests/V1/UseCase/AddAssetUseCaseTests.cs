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
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using Microsoft.Extensions.Logging.Abstractions;
using AssetInformationApi.V1.Boundary.Request;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class NewAssetUseCaseTests
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly NewAssetUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ISnsGateway> _assetSnsGateway;
        private readonly AssetSnsFactory _assetSnsFactory;

        public NewAssetUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _assetSnsGateway = new Mock<ISnsGateway>();
            _assetSnsFactory = new AssetSnsFactory();
            _classUnderTest = new NewAssetUseCase(_mockGateway.Object, _assetSnsGateway.Object, _assetSnsFactory, new NullLogger<NewAssetUseCase>());
        }

        [Fact]
        public async Task AddAssetUsecaseShouldReturnOkResponse()
        {
            var newAssetRequest = _fixture.Create<AddAssetRequest>();
            newAssetRequest.Id = Guid.NewGuid();
            var token = new Token();
            var assetDb = DynamoDbGateway.NewAssetRequestToDatabase(newAssetRequest);
            var assetToDomain = assetDb.ToDomain();

            _mockGateway.Setup(x => x.AddAsset(newAssetRequest).ReturnsAsync(assetDb);

            var response = await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);
            response.Should().BeEquivalentTo(asset.ToResponse());
        }

        [Fact]
        public async Task AddAssetUsecaseShouldReturnNull()
        {
            var newAssetRequest = _fixture.Create<AddAssetRequest>();
            var token = new Token();
            token.Email = "test@test.com";
            token.Name = "Test";
            _mockGateway.Setup(x => x.AddAsset(newAssetRequest)).ReturnsAsync(newAssetRequest);

            var response = await _classUnderTest.PostAsync(newAssetRequest, token).ConfigureAwait(false);
            response.Should().BeNull();
        }
    }
}
