using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Response;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetAssetByIdUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(GetAssetByIdRequest query);
    }
}
