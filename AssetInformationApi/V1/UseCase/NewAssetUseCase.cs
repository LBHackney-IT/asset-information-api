using AssetInformationApi.V1.Gateways;
using AssetInformationApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;

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
            var asset = new AssetDb()
            {
                Id = request.Id,
                AssetId = request.AssetId,
                AssetLocation = request.AssetLocation,
                AssetType = request.AssetType,
                ParentAssetIds = request.ParentAssetIds,
                RootAsset = request.RootAsset,
                Tenure = new AssetTenureDb {
                    Id = request.Tenure.Id,
                    Type = request.Tenure.Type,
                    PaymentReference = request.Tenure.PaymentReference,
                    StartOfTenureDate = request.Tenure.StartOfTenureDate,
                    EndOfTenureDate = request.Tenure.EndOfTenureDate
                },
                VersionNumber = request.VersionNumber,
                AssetAddress = request.AssetAddress,
                AssetCharacteristics = request.AssetCharacteristics,
                AssetManagement = request.AssetManagement
            };

           return await _gateway.AddAsset(asset).ConfigureAwait(false);
        }
    }
}
