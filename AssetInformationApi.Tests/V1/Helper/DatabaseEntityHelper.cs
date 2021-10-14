using AutoFixture;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.Tests.V1.Helper
{
    public static class DatabaseEntityHelper
    {
        public static AssetDb CreateDatabaseEntity()
        {
            var entity = new Fixture().Create<Asset>();

            return CreateDatabaseEntityFrom(entity);
        }

        public static AssetDb CreateDatabaseEntityFrom(Asset entity)
        {
            return entity.ToDatabase();
        }
    }
}
