using AutoFixture;
using AssetInformationApi.V1.Domain;
using AssetInformationApi.V1.Infrastructure;
using AssetInformationApi.V1.Factories;

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
