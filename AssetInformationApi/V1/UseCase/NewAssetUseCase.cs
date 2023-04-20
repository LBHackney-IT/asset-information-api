using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Factories;
using System;
using Hackney.Core.JWT;
using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Helpers;

namespace AssetInformationApi.V1.UseCase
{
    public class NewAssetUseCase : INewAssetUseCase
    {
        private readonly IAssetGateway _gateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;

        public NewAssetUseCase(IAssetGateway gateway, ISnsGateway snsGateway, ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        [LogCall]
        public async Task<AssetResponseObject> PostAsync(AddAssetRequest request, Token token)
        {
            if (PostCodeHelpers.SearchTextIsValidPostCode(request.AssetAddress.PostCode))
            {
                request.AssetAddress.PostCode = PostCodeHelpers.NormalizePostcode(request.AssetAddress.PostCode);
            }

            var asset = await _gateway.AddAsset(request.ToDatabase()).ConfigureAwait(false);
            if (asset != null && token != null)
            {
                var assetSnsMessage = _snsFactory.CreateAsset(asset, token);
                var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");
                await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);
            }


            return asset.ToResponse();
        }
    }
}
