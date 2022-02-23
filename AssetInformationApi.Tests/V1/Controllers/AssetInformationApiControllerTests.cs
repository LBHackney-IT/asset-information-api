using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Response;
using AssetInformationApi.V1.Controllers;
using AssetInformationApi.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AssetInformationApi.Tests.V1.Controllers
{
    [Collection("LogCall collection")]
    public class AssetInformationApiControllerTests
    {
        private readonly AssetInformationApiController _classUnderTest;
        private readonly Mock<IGetAssetByIdUseCase> _mockGetAssetByIdUseCase;
        private readonly Mock<IGetAssetByAssetIdUseCase> _mockGetAssetByAssetIdUseCase;
        private readonly Fixture _fixture = new Fixture();

        public AssetInformationApiControllerTests()
        {
            _mockGetAssetByIdUseCase = new Mock<IGetAssetByIdUseCase>();
            _mockGetAssetByAssetIdUseCase = new Mock<IGetAssetByAssetIdUseCase>();

            _classUnderTest = new AssetInformationApiController(
                _mockGetAssetByIdUseCase.Object,
                _mockGetAssetByAssetIdUseCase.Object);
        }

        private static GetAssetByIdRequest ConstructRequest(Guid? id = null)
        {
            return new GetAssetByIdRequest() { Id = id ?? Guid.NewGuid() };
        }

        [Fact]
        public async Task GetTenureWithNoIdReturnsNotFound()
        {
            var request = ConstructRequest();
            _mockGetAssetByIdUseCase.Setup(x => x.ExecuteAsync(request)).ReturnsAsync((AssetResponseObject) null);

            var response = await _classUnderTest.GetAssetById(request).ConfigureAwait(false);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(request.Id);
        }

        [Fact]
        public async Task GetTenureWithValidIdReturnsOKResponse()
        {
            var tenureResponse = _fixture.Create<AssetResponseObject>();
            var request = ConstructRequest(tenureResponse.Id);
            _mockGetAssetByIdUseCase.Setup(x => x.ExecuteAsync(request)).ReturnsAsync(tenureResponse);

            var response = await _classUnderTest.GetAssetById(request).ConfigureAwait(false);
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().Be(tenureResponse);
        }

        [Fact]
        public async Task GetAssetByAssetIdWhenEntityDoesntExistReturns404()
        {
            // Arrange
            var query = new GetAssetByAssetIdRequest
            {
                AssetId = _fixture.Create<string>()
            };

            // Act
            var response = await _classUnderTest.GetAssetByAssetId(query).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(query.AssetId);
        }

        [Fact]
        public async Task GetAssetByAssetIdWhenEntityExistsReturnsEntity()
        {
            // Arrange
            var useCaseResponse = _fixture.Create<AssetResponseObject>();

            _mockGetAssetByAssetIdUseCase
                .Setup(x => x.ExecuteAsync(It.IsAny<GetAssetByAssetIdRequest>()))
                .ReturnsAsync(useCaseResponse);

            var query = new GetAssetByAssetIdRequest
            {
                AssetId = useCaseResponse.AssetId
            };

            // Act
            var response = await _classUnderTest.GetAssetByAssetId(query).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().BeOfType(typeof(AssetResponseObject));

            ((response as OkObjectResult).Value as AssetResponseObject).Should().BeEquivalentTo(useCaseResponse);
        }
    }
}
