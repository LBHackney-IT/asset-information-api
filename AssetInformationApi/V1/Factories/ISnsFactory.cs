using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Infrastructure;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.V1.Factories
{
    public interface ISnsFactory
    {
        EntityEventSns CreateAsset(AddAssetRequest asset, Token token);
        EntityEventSns UpdateAsset(UpdateEntityResult<AssetDb> updateResult, Token token);
    }
}
