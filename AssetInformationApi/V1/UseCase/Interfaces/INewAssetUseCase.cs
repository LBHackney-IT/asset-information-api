using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface INewAssetUseCase
    {
        Task<Asset> PostAsync(Asset request);
    }
}
