using AssetInformationApi.V1.Boundary.Request;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;
using System;
using AssetInformationApi.V1.Infrastructure;
using Hackney.Shared.Asset.Boundary.Request;

namespace AssetInformationApi.V1.Gateways
{
    public interface IAssetGateway
    {
        Task<Asset> GetAssetByIdAsync(GetAssetByIdRequest query);
        Task<Asset> GetAssetByAssetId(GetAssetByAssetIdRequest query);
        Task<Asset> AddAsset(AssetDb asset);
        Task<UpdateEntityResult<AssetDb>> EditAssetDetails(Guid assetId, EditAssetRequest assetRequestObject, string requestBody, int? ifMatch);
        Task<UpdateEntityResult<AssetDb>> EditAssetAddressDetails(Guid assetId, EditAssetAddressRequest assetRequestObject, string requestBody, int? ifMatch);

    }
}
