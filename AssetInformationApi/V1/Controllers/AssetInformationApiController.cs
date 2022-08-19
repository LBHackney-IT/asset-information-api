using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Core.Http;
using System;

namespace AssetInformationApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/assets")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class AssetInformationApiController : BaseController
    {
        private readonly IGetAssetByIdUseCase _getAssetByIdUseCase;
        private readonly IGetAssetByAssetIdUseCase _getAssetByAssetIdUseCase;
        private readonly INewAssetUseCase _newAssetUseCase;
        private readonly ITokenFactory _tokenFactory;
        private readonly IHttpContextWrapper _contextWrapper;

        public AssetInformationApiController(
            IGetAssetByIdUseCase getAssetByIdUseCase,
            IGetAssetByAssetIdUseCase getAssetByAssetIdUseCase, INewAssetUseCase newAssetUseCase,
            ITokenFactory tokenFactory, IHttpContextWrapper contextWrapper)
        {
            _getAssetByIdUseCase = getAssetByIdUseCase;
            _getAssetByAssetIdUseCase = getAssetByAssetIdUseCase;
            _newAssetUseCase = newAssetUseCase;
            _tokenFactory = tokenFactory;
            _contextWrapper = contextWrapper;
        }

        /// <summary>
        /// Retrieves the asset with the supplied id
        /// </summary>
        /// <response code="200">Successfully retrieved details for the specified ID</response>
        /// <response code="404">No tenure information found for the specified ID</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(AssetResponseObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("{id}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetAssetById([FromRoute] GetAssetByIdRequest query)
        {
            var result = await _getAssetByIdUseCase.ExecuteAsync(query).ConfigureAwait(false);
            if (result == null) return NotFound(query.Id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the asset with the supplied assetId. Endpoint intended for RepairsAPI
        /// </summary>
        /// <response code="200">Successfully retrieved details for the specified AssetId</response>
        /// <response code="404">No tenure information found for the specified AssetId</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(AssetResponseObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("assetId/{assetId}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetAssetByAssetId([FromRoute] GetAssetByAssetIdRequest query)
        {
            var result = await _getAssetByAssetIdUseCase.ExecuteAsync(query).ConfigureAwait(false);
            if (result == null) return NotFound(query.AssetId);

            return Ok(result);
        }

        [ProducesResponseType(typeof(AssetResponseObject), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> AddAsset([FromBody] AddAssetRequest asset)
        {
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));
            var result = await _newAssetUseCase.PostAsync(asset.ToDatabase(), token).ConfigureAwait(false);

            return Created(new Uri($"api/v1/assets/{asset.Id}", UriKind.Relative), result);
        }
    }
}
