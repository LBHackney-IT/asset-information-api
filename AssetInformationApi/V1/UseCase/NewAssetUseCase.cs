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
using Newtonsoft.Json;
using AssetInformationApi.V1.Infrastructure;
using AssetInformationApi.V1.Boundary.Request;

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
        public async Task<AssetResponseObject> PostAsync(AddAssetRequest request, Token token)
        {
            _logger.LogDebug($"NewAssetUseCase - Calling _gateway.AddAsset for asset ID {request.Id}");

            var asset = await _gateway.AddAsset(request.ToDatabase()).ConfigureAwait(false);
            if (asset != null && token != null)
            {
                var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");

                var assetSnsMessage = _snsFactory.CreateAsset(asset, token);

                _logger.LogInformation("Publishing AssetCreatedEvent SNS message for new asset with prop ref: {AssetId}.", asset.AssetId);
                await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);

                if (request.AddDefaultSorContracts)
                {
                    var addRepairsContractsToNewAssetObject = new AddRepairsContractsToNewAssetObject()
                    {
                        EntityId = request.Id,
                        PropRef = request.AssetId,
                    };

                    var assetContractsSnsMessage = _snsFactory.AddRepairsContractsToNewAsset(addRepairsContractsToNewAssetObject, token);

                    _logger.LogInformation("Publishing AddRepairsContractsToAssetEvent SNS message for asset with prop ref: {AssetId}.", asset.AssetId);
                    await _snsGateway.Publish(assetContractsSnsMessage, assetTopicArn).ConfigureAwait(false);
                }
            }

            _logger.LogDebug($"NewAssetUseCase - New asset added. Converting newly added AssetDb object back to domain object (ref. asset ID {request.Id})");

            return asset.ToResponse();
        }
    }
}
