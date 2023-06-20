using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.Infrastructure
{
    public class EditAssetAddressDatabase : EditAssetDatabase
    {
        public AssetAddress AssetAddress { get; set; }
    }
}
