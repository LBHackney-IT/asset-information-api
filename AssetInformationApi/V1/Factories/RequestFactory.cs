using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Infrastructure;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;

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
        public static AssetDb ToDatabase(this AddAssetRequest domainEntity)
        {
            if (domainEntity == null) return null;

            return new AssetDb
            {
                Id = domainEntity.Id,
                AssetId = domainEntity.AssetId,
                AssetType = domainEntity.AssetType,
                RentGroup = domainEntity.RentGroup,
                RootAsset = domainEntity.RootAsset,
                IsActive = domainEntity.IsActive,
                ParentAssetIds = domainEntity.ParentAssetIds,
                BoilerHouseId = domainEntity.BoilerHouseId,
                AssetLocation = domainEntity.AssetLocation,
                AssetAddress = domainEntity.AssetAddress,
                AssetManagement = domainEntity.AssetManagement,
                AssetCharacteristics = domainEntity.AssetCharacteristics.ToDatabase(),
                Tenure = domainEntity.Tenure.ToDatabase(),
                VersionNumber = domainEntity.VersionNumber,
                Patches = domainEntity.Patches?.ToDatabase()
            };
        }
    }
}
