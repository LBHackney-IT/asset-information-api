using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Controllers;
using AssetInformationApi.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Asset.Boundary.Response;
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
        private readonly Fixture _fixture = new Fixture();

        public AssetInformationApiControllerTests()
        {
            _mockGetAssetByIdUseCase = new Mock<IGetAssetByIdUseCase>();
            _classUnderTest = new AssetInformationApiController(_mockGetAssetByIdUseCase.Object);
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
    }
}
