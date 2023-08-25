using System;

namespace AssetInformationApi.V1.Infrastructure
{
    public class AddRepairsContractsToNewAssetObject
    {
        public Guid EntityId { get; set; }
        public string PropRef { get; set; }
        public bool AddRepairsContracts { get; set; }
    }
}
