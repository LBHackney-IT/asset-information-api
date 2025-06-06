using AssetInformationApi.V1.Factories;
using AssetInformationApi.V1.Gateways.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Boundary.Response;
using System.Threading.Tasks;
using System;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Linq;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.V1.UseCase
{
    public class EditPropertyPatchUseCase : IEditPropertyPatchUseCase
    {
        private readonly IAssetGateway _assetGateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;

        public EditPropertyPatchUseCase(IAssetGateway assetGateway, ISnsGateway snsGateway, ISnsFactory snsFactory)
        {
            _assetGateway = assetGateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        public async Task<AssetResponseObject> ExecuteAsync(
            Guid assetId, EditPropertyPatchRequest assetRequestObject, string requestBody, Token token, int? ifMatch)
        {
            var result = await _assetGateway.EditAssetDetails(assetId, assetRequestObject, requestBody, ifMatch).ConfigureAwait(false);
            if (result == null) return null;

            if (result.NewValues.Any())
            {
                var assetSnsMessage = _snsFactory.UpdateAsset(result, token);
                var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");
                await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);
            }

            return result.UpdatedEntity.ToDomain().ToResponse();
        }
    }
}
