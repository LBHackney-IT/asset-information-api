using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.Boundary.Request
{
    public class AddAssetRequest : Asset
    {
        public bool AddDefaultSorContracts = false;
    }
}
