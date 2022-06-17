using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface INewAssetUseCase
    {
        Task<AssetResponseObject> PostAsync(Asset request);
    }
}
