using System;

namespace AssetInformationApi.V1.Boundary.Response
{
    public class AssetTenureResponseObject
    {
        public string Id { get; set; }
        public string PaymentReference { get; set; }
        public string Type { get; set; }
        public DateTime StartOfTenureDate { get; set; }
        public DateTime? EndOfTenureDate { get; set; }
        public bool IsActive { get; set; }
    }
}
