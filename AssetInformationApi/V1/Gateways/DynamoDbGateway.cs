using Amazon.DynamoDBv2.DataModel;
using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Domain;
using AssetInformationApi.V1.Factories;
using AssetInformationApi.V1.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
    }
}
