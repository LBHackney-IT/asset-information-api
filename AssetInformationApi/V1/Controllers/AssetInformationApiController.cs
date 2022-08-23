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
using Hackney.Shared.Asset.Infrastructure;
using System;
using Hackney.Core.Middleware;
using HeaderConstants = AssetInformationApi.V1.Infrastructure.HeaderConstants;
using System.Net.Http.Headers;
using AssetInformationApi.V1.Infrastructure.Exceptions;
using Hackney.Shared.Asset.Boundary.Request;

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
        private readonly IEditAssetUseCase _editAssetUseCase;
        private readonly ITokenFactory _tokenFactory;
        private readonly IHttpContextWrapper _contextWrapper;

        public AssetInformationApiController(
            IGetAssetByIdUseCase getAssetByIdUseCase,
            IGetAssetByAssetIdUseCase getAssetByAssetIdUseCase, INewAssetUseCase newAssetUseCase,
            ITokenFactory tokenFactory, IHttpContextWrapper contextWrapper, IEditAssetUseCase editAssetUseCase)
        {
            _getAssetByIdUseCase = getAssetByIdUseCase;
            _getAssetByAssetIdUseCase = getAssetByAssetIdUseCase;
            _newAssetUseCase = newAssetUseCase;
            _tokenFactory = tokenFactory;
            _contextWrapper = contextWrapper;
            _editAssetUseCase = editAssetUseCase;
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

        [ProducesResponseType(typeof(AssetResponseObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("add")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> AddAsset([FromBody] AddAssetRequest asset)
        {
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));
            var result = await _newAssetUseCase.PostAsync(asset.ToDatabase(), token).ConfigureAwait(false);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{id}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> PatchAsset([FromRoute] EditAssetByIdRequest query, [FromBody] EditAssetRequest asset)
        {
            var bodyText = await HttpContext.Request.GetRawBodyStringAsync().ConfigureAwait(false);
            var ifMatch = GetIfMatchFromHeader();
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));
            try
            {
                var result = await _editAssetUseCase.ExecuteAsync(query.Id, asset, bodyText, token, ifMatch).ConfigureAwait(false);

                if (result == null) return NotFound();

                return NoContent();
            }
            catch (VersionNumberConflictException vncErr)
            {
                return Conflict(vncErr.Message);
            }
        }

        private int? GetIfMatchFromHeader()
        {
            var header = HttpContext.Request.Headers.GetHeaderValue(HeaderConstants.IfMatch);

            if (header == null)
                return null;

            _ = EntityTagHeaderValue.TryParse(header, out var entityTagHeaderValue);

            if (entityTagHeaderValue == null)
                return null;

            var version = entityTagHeaderValue.Tag.Replace("\"", string.Empty);

            if (int.TryParse(version, out var numericValue))
                return numericValue;

            return null;
        }
    }
}
