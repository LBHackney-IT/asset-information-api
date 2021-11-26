using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetAssetByAssetIdUseCase
    {
        Task<AssetResponseObject> ExecuteAsync(GetAssetByAssetIdRequest query);
    }
}
