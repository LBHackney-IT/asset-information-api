using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Boundary.Response;

namespace AssetInformationApi.V1.UseCase
{
    public class NewAssetUseCase : INewAssetUseCase
    {
        private readonly IAssetGateway _gateway;

        public NewAssetUseCase(IAssetGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<AssetResponseObject> PostAsync(Asset request)
        {
            return (await _gateway.AddAsset(request.ToDatabase()).ConfigureAwait(false)).ToResponse();
        }
    }
}
