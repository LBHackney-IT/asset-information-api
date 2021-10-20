using System;

namespace AssetInformationApi.V1.Domain
{
    public class ParentAsset
    {
        public string Type { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
