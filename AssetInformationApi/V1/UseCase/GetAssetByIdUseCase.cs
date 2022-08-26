using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.UseCase
{
    public class GetAssetByIdUseCase : IGetAssetByIdUseCase
    {
        private readonly IAssetGateway _gateway;

        public GetAssetByIdUseCase(IAssetGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task<Asset> ExecuteAsync(GetAssetByIdRequest query)
        {
            return (await _gateway.GetAssetByIdAsync(query).ConfigureAwait(false));
        }
    }
}
