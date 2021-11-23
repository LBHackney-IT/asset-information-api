using Microsoft.AspNetCore.Mvc;

namespace AssetInformationApi.V1.Boundary.Request
{
    public class GetAssetByAssetIdRequest
    {
        [FromRoute(Name = "assetId")]
        public string AssetId { get; set; }
    }
}
