using AssetInformationApi.V1.Boundary.Request;
using Hackney.Shared.Asset.Boundary.Response;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetAssetByIdUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(GetAssetByIdRequest query);
    }
}
