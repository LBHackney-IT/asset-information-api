using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Controllers;
using AssetInformationApi.V1.UseCase.Interfaces;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Xunit;
using Hackney.Shared.Asset.Domain;
using Hackney.Core.JWT;
using Hackney.Core.Http;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Factories;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Hackney.Shared.Asset.Boundary.Request;
using AssetInformationApi.V1.Infrastructure.Exceptions;
using TestStack.BDDfy;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssetInformationApi.Tests.V1.Controllers
{
    [Collection("LogCall collection")]
    public class AssetInformationApiControllerTests
    {
        private readonly AssetInformationApiController _classUnderTest;
        private readonly Mock<IGetAssetByIdUseCase> _mockGetAssetByIdUseCase;
        private readonly Mock<IGetAssetByAssetIdUseCase> _mockGetAssetByAssetIdUseCase;
        private readonly Mock<INewAssetUseCase> _mockAddNewAssetUseCase;
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Mock<IHttpContextWrapper> _mockContextWrapper;
        private readonly Mock<IEditAssetUseCase> _mockEditAssetUseCase;
        private readonly Mock<IEditAssetAddressUseCase> _mockEditAssetAddressUseCase;
        private readonly Mock<IEditPropertyPatchUseCase> _mockEditPropertyPatchUseCase;

        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly HeaderDictionary _requestHeaders;
        private readonly Mock<HttpResponse> _mockHttpResponse;
        private readonly HeaderDictionary _responseHeaders;

        private const string RequestBodyText = "Some request body text";
        private readonly MemoryStream _requestStream;

        public AssetInformationApiControllerTests()
        {
            _mockGetAssetByIdUseCase = new Mock<IGetAssetByIdUseCase>();
            _mockGetAssetByAssetIdUseCase = new Mock<IGetAssetByAssetIdUseCase>();
            _mockAddNewAssetUseCase = new Mock<INewAssetUseCase>();
            _mockTokenFactory = new Mock<ITokenFactory>();
            _mockContextWrapper = new Mock<IHttpContextWrapper>();
            _mockEditAssetUseCase = new Mock<IEditAssetUseCase>();
            _mockEditAssetAddressUseCase = new Mock<IEditAssetAddressUseCase>();
            _mockEditPropertyPatchUseCase = new Mock<IEditPropertyPatchUseCase>();

            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHttpResponse = new Mock<HttpResponse>();

            _classUnderTest = new AssetInformationApiController(
                _mockGetAssetByIdUseCase.Object,
                _mockGetAssetByAssetIdUseCase.Object,
                _mockAddNewAssetUseCase.Object,
                _mockTokenFactory.Object,
                _mockContextWrapper.Object,
                _mockEditAssetUseCase.Object,
                _mockEditAssetAddressUseCase.Object,
                _mockEditPropertyPatchUseCase.Object,
                new NullLogger<AssetInformationApiController>()
                );

            // changes to allow reading of raw request body
            _requestStream = new MemoryStream(Encoding.Default.GetBytes(RequestBodyText));
            _mockHttpRequest.SetupGet(x => x.Body).Returns(_requestStream);


            _requestHeaders = new HeaderDictionary();
            _mockHttpRequest.SetupGet(x => x.Headers).Returns(_requestHeaders);

            _mockContextWrapper
                .Setup(x => x.GetContextRequestHeaders(It.IsAny<HttpContext>()))
                .Returns(_requestHeaders);

            _responseHeaders = new HeaderDictionary();
            _mockHttpResponse.SetupGet(x => x.Headers).Returns(_responseHeaders);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(x => x.Request).Returns(_mockHttpRequest.Object);
            mockHttpContext.SetupGet(x => x.Response).Returns(_mockHttpResponse.Object);

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            _classUnderTest.ControllerContext = controllerContext;


        }

        private static GetAssetByIdRequest ConstructRequest(Guid? id = null)
        {
            return new GetAssetByIdRequest() { Id = id ?? Guid.NewGuid() };
        }

        [Fact]
        public async Task GetTenureWithNoIdReturnsNotFound()
        {
            var request = ConstructRequest();
            _mockGetAssetByIdUseCase.Setup(x => x.ExecuteAsync(request)).ReturnsAsync((Asset) null);

            var response = await _classUnderTest.GetAssetById(request).ConfigureAwait(false);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(request.Id);
        }

        [Fact]
        public async Task GetTenureWithValidIdReturnsOKResponse()
        {
            var tenureDomain = _fixture.Create<Asset>();
            var tenureResponse = tenureDomain.ToResponse();
            var request = ConstructRequest(tenureDomain.Id);
            _mockGetAssetByIdUseCase.Setup(x => x.ExecuteAsync(request)).ReturnsAsync(tenureDomain);

            var response = await _classUnderTest.GetAssetById(request).ConfigureAwait(false);
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().BeEquivalentTo(tenureResponse);
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

        [Fact]
        public async Task SaveAssetReturnsEntity()
        {
            // Arrange
            var useCaseResponse = _fixture.Create<Asset>();
            var expectedControllerResponse = useCaseResponse.ToResponse();

            _mockGetAssetByIdUseCase
                .Setup(x => x.ExecuteAsync(It.IsAny<GetAssetByIdRequest>()))
                .ReturnsAsync(useCaseResponse);

            var useCase = _fixture.Create<AddAssetRequest>();
            var Id = Guid.NewGuid();
            useCase.Id = Id;
            var query = new GetAssetByIdRequest
            {
                Id = Id
            };

            // Act
            await _classUnderTest.AddAsset(useCase).ConfigureAwait(false);
            var response = await _classUnderTest.GetAssetById(query).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(OkObjectResult));

            ((response as OkObjectResult).Value).Should().BeEquivalentTo(expectedControllerResponse);
        }

        [Fact]
        public async Task EditAssetWhenValidReturns204NoContentResponse()
        {
            var mockQuery = _fixture.Create<EditAssetRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditAssetUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>())).ReturnsAsync(_fixture.Create<AssetResponseObject>());

            var response = await _classUnderTest.PatchAsset(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NoContentResult));
        }

        [Fact]
        public async Task EditAssetWhenAssetDoesntExistReturns404NotFoundResponse()
        {
            var mockQuery = _fixture.Create<EditAssetRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditAssetUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>())).ReturnsAsync((AssetResponseObject) null);

            var response = await _classUnderTest.PatchAsset(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NotFoundResult));
        }

        [Fact]
        public async Task EditAssetAddressWhenValidReturns204Response()
        {
            var mockQuery = _fixture.Create<EditAssetAddressRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();
            EditAssetAddressRequest calledRequest = null;

            _mockEditAssetAddressUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ReturnsAsync(_fixture.Create<AssetResponseObject>())
                .Callback<Guid, EditAssetAddressRequest, string, Token, int?>((g, r, req, t, m) => calledRequest = r);

            var response = await _classUnderTest.PatchAssetAddress(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NoContentResult));
            calledRequest.Should().BeEquivalentTo(mockQuery);
            _mockEditAssetAddressUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Once);
            _mockEditAssetUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task EditAssetAddressWhenValidCallsTheCorrectUseCaseCorrectly()
        {
            var mockQuery = _fixture.Create<EditAssetAddressRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();
            EditAssetAddressRequest calledRequest = null;

            _mockEditAssetAddressUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ReturnsAsync(_fixture.Create<AssetResponseObject>())
                .Callback<Guid, EditAssetAddressRequest, string, Token, int?>((g, r, req, t, m) => calledRequest = r);

            var response = await _classUnderTest.PatchAssetAddress(mockRequestObject, mockQuery).ConfigureAwait(false);

            _mockEditAssetAddressUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Once);
            _mockEditAssetUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task EditAssetAddressWhenAssetDoesntExistReturns404NotFoundResponse()
        {
            var mockQuery = _fixture.Create<EditAssetAddressRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditAssetAddressUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>())).ReturnsAsync((AssetResponseObject) null);

            var response = await _classUnderTest.PatchAssetAddress(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NotFoundResult));
        }

        [Fact]
        public async Task EditAssetAddressReturns409WhenExceptionIsThrown()
        {
            var mockQuery = _fixture.Create<EditAssetAddressRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditAssetAddressUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetAddressRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .Throws(new VersionNumberConflictException(1, 2));

            var response = await _classUnderTest.PatchAssetAddress(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(ConflictObjectResult));
        }
        [Fact]
        public async Task EditPrpertyPatchWhenValidReturns204Response()
        {
            var mockQuery = _fixture.Create<EditPropertyPatchRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();
            EditPropertyPatchRequest calledRequest = null;

            _mockEditPropertyPatchUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ReturnsAsync(_fixture.Create<AssetResponseObject>())
                .Callback<Guid, EditPropertyPatchRequest, string, Token, int?>((g, r, req, t, m) => calledRequest = r);

            var response = await _classUnderTest.EditPropertyPatch(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NoContentResult));
            calledRequest.Should().BeEquivalentTo(mockQuery);
            _mockEditPropertyPatchUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Once);
            _mockEditAssetUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditAssetRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task EditPropertyPatchWhenAssetDoesntExistReturns404NotFoundResponse()
        {
            var mockQuery = _fixture.Create<EditPropertyPatchRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditPropertyPatchUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>())).ReturnsAsync((AssetResponseObject) null);

            var response = await _classUnderTest.EditPropertyPatch(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(NotFoundResult));
        }

        [Fact]
        public async Task EditPropertyPatchReturns409WhenExceptionIsThrown()
        {
            var mockQuery = _fixture.Create<EditPropertyPatchRequest>();
            var mockRequestObject = _fixture.Create<EditAssetByIdRequest>();

            _mockEditPropertyPatchUseCase.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<EditPropertyPatchRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .Throws(new VersionNumberConflictException(1, 2));

            var response = await _classUnderTest.EditPropertyPatch(mockRequestObject, mockQuery).ConfigureAwait(false);

            response.Should().BeOfType(typeof(ConflictObjectResult));
        }
    }
}
