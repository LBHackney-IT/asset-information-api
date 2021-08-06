using System.Collections.Generic;

namespace AssetInformationApi.V1.Domain
{
    public class AssetLocation
    {
        public int FloorNo { get; set; }
        public int TotalBlockFloors { get; set; }
        public IEnumerable<ParentAsset> ParentAssets { get; set; }
    }
}
