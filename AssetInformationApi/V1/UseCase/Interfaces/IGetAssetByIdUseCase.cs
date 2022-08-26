using AssetInformationApi.V1.Boundary.Request;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetAssetByIdUseCase
    {
        Task<Asset> ExecuteAsync(GetAssetByIdRequest query);
    }
}
