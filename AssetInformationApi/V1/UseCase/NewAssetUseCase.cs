using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.V1.UseCase
{
    public class NewAssetUseCase : INewAssetUseCase
    {
        private readonly IAssetGateway _gateway;

        public NewAssetUseCase(IAssetGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task<AssetResponseObject> PostAsync(AssetDb request)
        {
            return (await _gateway.AddAsset(request).ConfigureAwait(false)).ToResponse();
        }
    }
}
