using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.V1.UseCase
{
    public class NewAssetUseCase : INewAssetUseCase
    {
        private readonly IAssetGateway _gateway;

        public NewAssetUseCase(IAssetGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<Asset> PostAsync(Asset request)
        {
            return await _gateway.AddAsset(request.ToDatabase()).ConfigureAwait(false);
        }
    }
}
