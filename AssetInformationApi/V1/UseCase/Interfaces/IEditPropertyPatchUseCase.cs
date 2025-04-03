using Hackney.Core.JWT;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Boundary.Response;
using System.Threading.Tasks;
using System;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IEditPropertyPatchUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(
            Guid assetId, EditPropertyPatchRequest assetRequestObject, string requestBody, Token token, int? ifMatch);
    }
}
