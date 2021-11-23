using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.UseCase
{
    public class GetAssetByAssetIdUseCase : IGetAssetByAssetIdUseCase
    {
        private readonly IAssetGateway _gateway;

        public GetAssetByAssetIdUseCase(IAssetGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task<AssetResponseObject> ExecuteAsync(GetAssetByAssetIdRequest query)
        {
            var asset = await _gateway.GetAssetByAssetId(query).ConfigureAwait(false);

            return asset?.ToResponse();
        }
    }
}
