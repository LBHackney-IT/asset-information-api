using AssetInformationApi.V1.Boundary.Response;
using AssetInformationApi.V1.Domain;

namespace AssetInformationApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static AssetResponseObject ToResponse(this Asset domain)
        {
            if (domain == null) return null;
            return new AssetResponseObject()
            {
                Id = domain.Id,
                AssetId = domain.AssetId,
                AssetType = domain.AssetType,
                RootAsset = domain.RootAsset,
                ParentAssetIds = domain.ParentAssetIds,
                AssetLocation = domain.AssetLocation,
                AssetAddress = domain.AssetAddress,
                AssetManagement = domain.AssetManagement,
                AssetCharacteristics = domain.AssetCharacteristics,
                Tenure = domain.Tenure.ToResponse()
            };
        }

        public static AssetTenureResponseObject ToResponse(this AssetTenure domain)
        {
            if (domain == null) return null;
            return new AssetTenureResponseObject()
            {
                Id = domain.Id,
                PaymentReference = domain.PaymentReference,
                Type = domain.Type,
                StartOfTenureDate = domain.StartOfTenureDate,
                EndOfTenureDate = domain.EndOfTenureDate,
                IsActive = domain.IsActive
            };
        }
    }
}
