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

namespace AssetInformationApi.V1.Gateways
{
    public class DynamoDbGateway : IAssetGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbGateway> _logger;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
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
            _dynamoDbContext.SaveAsync(asset).GetAwaiter().GetResult();

            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {asset.Id}");
            var result = await _dynamoDbContext.LoadAsync<AssetDb>(asset.Id).ConfigureAwait(false);

            return result?.ToDomain();
        }
    }
}
