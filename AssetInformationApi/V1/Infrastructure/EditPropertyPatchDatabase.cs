using System;

namespace AssetInformationApi.V1.Infrastructure
{
    public class EditPropertyPatchDatabase : EditAssetDatabase
    {
        public Guid AreaId { get; set; }
        public Guid PatchId { get; set; }
    }
}
