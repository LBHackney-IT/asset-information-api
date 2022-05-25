using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface ISaveAssetUseCase
    {
        Task<Asset> PostAsync(Asset request);
    }
}
