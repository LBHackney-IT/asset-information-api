using AssetInformationApi.V1.Domain;
using System;

namespace AssetInformationApi.V1.Boundary.Response
{
    public class AssetResponseObject
    {
        public Guid Id { get; set; }
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string RootAsset { get; set; }
        public string ParentAssetIds { get; set; }

        public AssetLocation AssetLocation { get; set; }
        public AssetAddress AssetAddress { get; set; }
        public AssetManagement AssetManagement { get; set; }
        public AssetCharacteristics AssetCharacteristics { get; set; }
        public AssetTenureResponseObject Tenure { get; set; }
    }
}
