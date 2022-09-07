using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase;
using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Xunit;

namespace AssetInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class GetAssetByIdUseCaseTests
    {
        private readonly Mock<IAssetGateway> _mockGateway;
        private readonly GetAssetByIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public GetAssetByIdUseCaseTests()
        {
            _mockGateway = new Mock<IAssetGateway>();
            _classUnderTest = new GetAssetByIdUseCase(_mockGateway.Object);
        }

        private static GetAssetByIdRequest ConstructRequest(Guid? id = null)
        {
            return new GetAssetByIdRequest() { Id = id ?? Guid.NewGuid() };
        }

        [Fact]
        public async Task GetByIdUsecaseShouldBeNull()
        {
            var request = ConstructRequest();
            _mockGateway.Setup(x => x.GetAssetByIdAsync(request)).ReturnsAsync((Asset) null);

            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(false);
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdUsecaseShouldReturnOkResponse()
        {
            var asset = _fixture.Create<Asset>();
            var request = ConstructRequest(asset.Id);
            _mockGateway.Setup(x => x.GetAssetByIdAsync(request)).ReturnsAsync(asset);


            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(false);
            response.Should().BeEquivalentTo(asset.ToResponse());
        }

        [Fact]
        public void GetByIdThrowsException()
        {
            var request = ConstructRequest();
            var exception = new ApplicationException("Test Exception");
            _mockGateway.Setup(x => x.GetAssetByIdAsync(request)).ThrowsAsync(exception);
            Func<Task<Asset>> throwException = async () => await _classUnderTest.ExecuteAsync(request).ConfigureAwait(false);
            throwException.Should().Throw<ApplicationException>().WithMessage("Test Exception");
        }
    }
}
