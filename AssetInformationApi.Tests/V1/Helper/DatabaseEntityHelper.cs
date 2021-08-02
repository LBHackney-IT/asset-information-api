using AutoFixture;
using AssetInformationApi.V1.Domain;
using AssetInformationApi.V1.Infrastructure;

namespace AssetInformationApi.Tests.V1.Helper
{
    public static class DatabaseEntityHelper
    {
        public static DatabaseEntity CreateDatabaseEntity()
        {
            var entity = new Fixture().Create<Entity>();

            return CreateDatabaseEntityFrom(entity);
        }

        public static DatabaseEntity CreateDatabaseEntityFrom(Entity entity)
        {
            return new DatabaseEntity
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
            };
        }
    }
}
