using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;
using System;
using AssetInformationApi.V1.Infrastructure;

namespace AssetInformationApi.V1.Factories
{
    public class AssetSnsFactory : ISnsFactory
    {
        public EntityEventSns CreateAsset(Asset asset, Token token)
        {
            return new EntityEventSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = asset.Id,
                Id = Guid.NewGuid(),
                EventType = CreateAssetEventConstants.EVENTTYPE,
                Version = CreateAssetEventConstants.V1_VERSION,
                SourceDomain = CreateAssetEventConstants.SOURCE_DOMAIN,
                SourceSystem = CreateAssetEventConstants.SOURCE_SYSTEM,
                EventData = new EventData
                {
                    NewData = asset
                },
                User = new User { Name = token.Name, Email = token.Email }
            };
        }
    }
}
