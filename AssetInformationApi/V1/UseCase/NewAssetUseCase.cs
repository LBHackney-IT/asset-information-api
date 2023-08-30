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
using AssetInformationApi.V1.Infrastructure;
using AssetInformationApi.V1.Boundary.Request;
using System.Text.Json;

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
            // Temporary - For troubleshooting purposes
            _logger.LogInformation("AddAssetRequest received for asset with prop ref: {AssetId}. AddAssetRequest body: {Request}", request.AssetId, JsonSerializer.Serialize(request));

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
                    // Temporary - For troubleshooting purposes
                    _logger.LogInformation("Assembling AddRepairsContractsToNewAssetObject object for asset with prop ref: {AssetId}", request.AssetId);

                    var addRepairsContractsToNewAssetObject = new AddRepairsContractsToNewAssetObject()
                    {
                        EntityId = request.Id,
                        PropRef = request.AssetId,
                    };

                    // Temporary - For troubleshooting purposes
                    _logger.LogInformation("Preparing SNS message before publishing AddRepairsContractsToAssetEvent for asset with prop ref: {AssetId}. AddRepairsContractsToNewAssetObject: {AddRepairsContractsToNewAssetObject}", request.AssetId, JsonSerializer.Serialize(addRepairsContractsToNewAssetObject));
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
