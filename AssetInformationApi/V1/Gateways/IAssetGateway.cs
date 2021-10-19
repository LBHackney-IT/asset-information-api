using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Domain;
using Hackney.Shared.Asset.Domain;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.Gateways
{
    public interface IAssetGateway
    {
        Task<Asset> GetAssetByIdAsync(GetAssetByIdRequest query);
    }
}
