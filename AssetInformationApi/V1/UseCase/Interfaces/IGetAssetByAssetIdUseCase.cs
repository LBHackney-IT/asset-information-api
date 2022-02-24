using AssetInformationApi.V1.Boundary.Request;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetAssetByAssetIdUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(GetAssetByAssetIdRequest query);
    }
}
