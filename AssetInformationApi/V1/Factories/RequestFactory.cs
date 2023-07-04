using AssetInformationApi.V1.Infrastructure;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.V1.Factories
{
    public static class EntityFactory
    {
        public static EditAssetDatabase ToDatabase(this EditAssetRequest domainEntity)
        {
            if (domainEntity == null) return null;

            return new EditAssetDatabase
            {
                RootAsset = domainEntity.RootAsset,
                ParentAssetIds = domainEntity.ParentAssetIds,
                BoilerHouseId = domainEntity.BoilerHouseId,
                IsActive = domainEntity.IsActive,
                AssetLocation = domainEntity.AssetLocation,
                AssetManagement = domainEntity.AssetManagement,
                AssetCharacteristics = domainEntity.AssetCharacteristics.ToDatabase()
            };
        }

        public static EditAssetAddressDatabase ToDatabase(this EditAssetAddressRequest domainEntity)
        {
            if (domainEntity == null) return null;

            return new EditAssetAddressDatabase
            {
                AssetAddress = domainEntity.AssetAddress,
                RootAsset = domainEntity.RootAsset,
                ParentAssetIds = domainEntity.ParentAssetIds,
                BoilerHouseId = domainEntity.BoilerHouseId,
                IsActive = domainEntity.IsActive,
                AssetLocation = domainEntity.AssetLocation,
                AssetManagement = domainEntity.AssetManagement,
                AssetCharacteristics = domainEntity.AssetCharacteristics.ToDatabase()
            };
        }
    }
}
