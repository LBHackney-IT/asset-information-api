using Hackney.Shared.Asset.Domain;
using System;

namespace AssetInformationApi.V1.Boundary.Request
{
    public class AddAssetRequest
    {
        public Guid Id { get; set; }
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public bool IsActive { get; set; }
        public string ParentAssetIds { get; set; }
        public AssetLocation AssetLocation { get; set; }
        public AssetAddress AssetAddress { get; set; }
        public AssetManagement AssetManagement { get; set; }
        public AssetCharacteristics AssetCharacteristics { get; set; }
        public int? VersionNumber { get; set; } = 0;
    }
}
