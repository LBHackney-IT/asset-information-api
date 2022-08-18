using System.Threading.Tasks;
using Hackney.Core.JWT;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface INewAssetUseCase
    {
        Task<AssetResponseObject> PostAsync(AssetDb request, Token token);
    }
}
