using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.V1.Infrastructure
{
    public class EditAssetDatabase
    {
        public string RootAsset { get; set; }

        public string ParentAssetIds { get; set; }
        public string BoilerHouseId { get; set; }

        public bool IsActive { get; set; }
        public AssetLocation AssetLocation { get; set; }

        public AssetManagement AssetManagement { get; set; }

        public AssetCharacteristicsDb AssetCharacteristics { get; set; }

    }
}
