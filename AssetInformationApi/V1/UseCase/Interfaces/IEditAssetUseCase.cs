using AssetInformationApi.V1.Boundary.Request;
using Hackney.Core.JWT;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Infrastructure;
using System;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IEditAssetUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(Guid assetId, EditAssetRequest assetRequestObject, string requestBody, Token token, int? ifMatch);
    }
}
