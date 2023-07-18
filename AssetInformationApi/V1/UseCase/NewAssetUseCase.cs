using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using System;
using Hackney.Core.JWT;
using Microsoft.Extensions.Logging;
using AssetInformationApi.V1.Gateways.Interfaces;

namespace AssetInformationApi.V1.UseCase
{
    public class NewAssetUseCase : INewAssetUseCase
    {
        private readonly IAssetGateway _gateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;
        private readonly ILogger<NewAssetUseCase> _logger;

        public NewAssetUseCase(IAssetGateway gateway, ISnsGateway snsGateway, ISnsFactory snsFactory, ILogger<NewAssetUseCase> logger)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
            _logger = logger;
        }

        [LogCall]
        public async Task<AssetResponseObject> PostAsync(AssetDb request, Token token)
        {
            _logger.LogDebug($"NewAssetUseCase - Calling _gateway.AddAsset for asset ID {request.Id}");

            var asset = await _gateway.AddAsset(request).ConfigureAwait(false);
            if (asset != null && token != null)
            {
                var assetSnsMessage = _snsFactory.CreateAsset(asset, token);
                var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");
                await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);
            }

            _logger.LogDebug($"NewAssetUseCase - New asset added. Converting newly added AssetDb object back to domain object (ref. asset ID {request.Id})");

            return asset.ToResponse();
        }
    }
}
