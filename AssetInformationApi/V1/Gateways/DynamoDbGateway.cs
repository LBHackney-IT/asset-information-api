using Amazon.DynamoDBv2.DataModel;
using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Domain;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using AssetInformationApi.V1.Infrastructure;
using System;
using AssetInformationApi.V1.Infrastructure.Exceptions;
using Hackney.Shared.Asset.Boundary.Request;
using System.Linq.Expressions;

namespace AssetInformationApi.V1.Gateways
{
    public class DynamoDbGateway : IAssetGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbGateway> _logger;
        private readonly IEntityUpdater _updater;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbGateway> logger, IEntityUpdater updater)
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
            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for id {asset.Id}");
            if (!string.IsNullOrEmpty(asset.AssetId))
            {
                GetAssetByAssetIdRequest getAssetByAssetIdRequest = new GetAssetByAssetIdRequest();
                getAssetByAssetIdRequest.AssetId = asset.AssetId;
                var assetById = GetAssetByAssetId(getAssetByAssetIdRequest);

                if (assetById != null)
                    throw new DuplicateAssetIdException(asset.AssetId);
            }
            _dynamoDbContext.SaveAsync(asset).GetAwaiter().GetResult();

            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {asset.Id}");
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
            if (!string.IsNullOrEmpty(existingAsset.AssetId))
            {
                GetAssetByAssetIdRequest getAssetByAssetIdRequest = new GetAssetByAssetIdRequest();
                getAssetByAssetIdRequest.AssetId = existingAsset.AssetId;
                var assetById = GetAssetByAssetId(getAssetByAssetIdRequest);

                if (assetById != null && assetById.Result.Id != assetId)
                    throw new DuplicateAssetIdException(existingAsset.AssetId);
            }
            var response = _updater.UpdateEntity(existingAsset, requestBody, assetRequestObject);

            if (response.NewValues.Any())
            {
                _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync to update id {assetId}");
                await _dynamoDbContext.SaveAsync<AssetDb>(response.UpdatedEntity).ConfigureAwait(false);
            }

            return response;
        }
    }
}
