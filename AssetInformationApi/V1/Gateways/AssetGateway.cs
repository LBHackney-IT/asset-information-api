using Amazon.DynamoDBv2.DataModel;
using AssetInformationApi.V1.Boundary.Request;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using AssetInformationApi.V1.Infrastructure;
using System;
using AssetInformationApi.V1.Infrastructure.Exceptions;
using Hackney.Shared.Asset.Boundary.Request;
using AssetInformationApi.V1.Helpers;
using AssetInformationApi.V1.Factories;

namespace AssetInformationApi.V1.Gateways
{
    public class AssetGateway : IAssetGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<AssetGateway> _logger;
        private readonly IEntityUpdater _updater;

        public AssetGateway(IDynamoDBContext dynamoDbContext, ILogger<AssetGateway> logger, IEntityUpdater updater)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
            _updater = updater;
        }

        [LogCall]
        public async Task<Asset> GetAssetByIdAsync(GetAssetByIdRequest query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {query.Id}");
            var result = await _dynamoDbContext.LoadAsync<AssetDb>(query.Id).ConfigureAwait(false);
            return result?.ToDomain();
        }

        [LogCall]
        public async Task<Asset> GetAssetByAssetId(GetAssetByAssetIdRequest query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for AssetId {query.AssetId}");

            var config = new DynamoDBOperationConfig
            {
                IndexName = "AssetId"
            };

            var search = _dynamoDbContext.QueryAsync<AssetDb>(query.AssetId, config);

            var response = await search.GetNextSetAsync().ConfigureAwait(false);
            if (response.Count == 0) return null;

            return response.First().ToDomain();
        }

        [LogCall]
        public async Task<Asset> AddAsset(AssetDb asset)
        {
            _logger.LogDebug($"DynamoDbGateway AddAsset - Checking and normalizing postcode prior to adding asset with ID {asset.Id})");

            if (PostcodeHelpers.IsValidPostCode(asset.AssetAddress.PostCode))
            {
                asset.AssetAddress.PostCode = PostcodeHelpers.NormalizePostcode(asset.AssetAddress.PostCode);
            }

            _logger.LogDebug($"DynamoDbGateway AddAsset - Calling IDynamoDBContext.SaveAsync for asset ID {asset.Id}");
            if (!string.IsNullOrEmpty(asset.AssetId))
            {
                GetAssetByAssetIdRequest getAssetByAssetIdRequest = new GetAssetByAssetIdRequest();
                getAssetByAssetIdRequest.AssetId = asset.AssetId;
                var assetById = await GetAssetByAssetId(getAssetByAssetIdRequest);

                if (assetById != null)
                    throw new DuplicateAssetIdException(asset.AssetId);
            }
            _dynamoDbContext.SaveAsync(asset).GetAwaiter().GetResult();

            _logger.LogDebug($"DynamoDbGateway AddAsset - Calling IDynamoDBContext.LoadAsync for asset ID {asset.Id}");

            var result = await _dynamoDbContext.LoadAsync<AssetDb>(asset.Id).ConfigureAwait(false);

            return result?.ToDomain();
        }

        [LogCall]
        public async Task<UpdateEntityResult<AssetDb>> EditAssetDetails(Guid assetId, EditAssetRequest assetRequestObject, string requestBody, int? ifMatch)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for id {assetId}");
            var existingAsset = await _dynamoDbContext.LoadAsync<AssetDb>(assetId).ConfigureAwait(false);
            if (existingAsset == null) return null;

            if (ifMatch != existingAsset.VersionNumber)
                throw new VersionNumberConflictException(ifMatch, existingAsset.VersionNumber);

            UpdateEntityResult<AssetDb> updaterResponse;

            if (assetRequestObject is EditAssetAddressRequest editAddressRequest && PostcodeHelpers.IsValidPostCode(editAddressRequest.AssetAddress.PostCode))
            {
                editAddressRequest.AssetAddress.PostCode = PostcodeHelpers.NormalizePostcode(editAddressRequest.AssetAddress.PostCode);
                updaterResponse = _updater.UpdateEntity<AssetDb, EditAssetAddressDatabase>(existingAsset, requestBody, editAddressRequest.ToDatabase());
            }
            else
            {
                updaterResponse = _updater.UpdateEntity<AssetDb, EditAssetDatabase>(existingAsset, requestBody, assetRequestObject.ToDatabase());
            }

            if (updaterResponse.NewValues.Any())
            {
                _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync to update id {assetId}");
                await _dynamoDbContext.SaveAsync<AssetDb>(updaterResponse.UpdatedEntity).ConfigureAwait(false);
            }

            return updaterResponse;
        }
    }
}
