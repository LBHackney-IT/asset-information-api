using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.V1.Factories
{
    public interface ISnsFactory
    {
        EntityEventSns CreateAsset(Asset asset, Token token);
    }
}
