using System;

namespace AssetInformationApi.V1.Infrastructure
{
    // The same class is also defined in Repairs Listener to ensure the information sent and received is in the same format
    public class AddRepairsContractsToNewAssetObject
    {
        public Guid EntityId { get; set; }
        public string PropRef { get; set; }
    }
}
