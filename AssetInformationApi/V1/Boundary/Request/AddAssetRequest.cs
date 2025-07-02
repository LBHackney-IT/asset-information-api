using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.V1.Boundary.Request
{
    public class AddAssetRequest : Asset
    {
        public bool AddDefaultSorContracts { get; set; } = false;
        public bool BypassPostcodeValidation { get; set; } = false;
    }
}
