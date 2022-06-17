using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;

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


        public AssetInformationApiController(
            IGetAssetByIdUseCase getAssetByIdUseCase,
            IGetAssetByAssetIdUseCase getAssetByAssetIdUseCase, INewAssetUseCase newAssetUseCase)
        {
            _getAssetByIdUseCase = getAssetByIdUseCase;
            _getAssetByAssetIdUseCase = getAssetByAssetIdUseCase;
            _newAssetUseCase = newAssetUseCase;
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
        public async Task<IActionResult> AddAsset([FromBody] Asset asset)
        {
            var result = await _newAssetUseCase.PostAsync(asset).ConfigureAwait(false);

            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
