using AutoFixture;
using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Shared.Asset.Infrastructure;
using System.Collections.Generic;

namespace AssetInformationApi.Tests.V1.UseCase
{
    public class EditAssetTestBase
    {
        protected readonly Fixture _fixture = new Fixture();

        protected UpdateEntityResult<AssetDb> MockUpdateEntityResultWhereChangesAreMade()
        {
            return new UpdateEntityResult<AssetDb>
            {
                UpdatedEntity = _fixture.Create<AssetDb>(),
                NewValues = new Dictionary<string, object>
                {
                    { "ParentAssetIds", _fixture.Create<string>() }
                }
            };
        }

        protected UpdateEntityResult<AssetDb> MockUpdateEntityResultWhereNoChangesAreMade()
        {
            return new UpdateEntityResult<AssetDb>
            {
                UpdatedEntity = _fixture.Create<AssetDb>()
                // empty
            };
        }
    }
}
