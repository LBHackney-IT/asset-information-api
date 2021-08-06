using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Response;
using AssetInformationApi.V1.Factories;
using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;

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
        public async Task<AssetResponseObject> ExecuteAsync(GetAssetByIdRequest query)
        {
            return (await _gateway.GetAssetByIdAsync(query).ConfigureAwait(false)).ToResponse();
        }
    }
}
