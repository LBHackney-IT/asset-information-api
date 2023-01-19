using System;

namespace AssetInformationApi.V1.Infrastructure.Exceptions
{
    public class DuplicateAssetIdException : Exception
    {
        public DuplicateAssetIdException(string assetId)
            : base(string.Format("Asset with AssetId ({0}) already exists.",
         assetId))
        {
        }
    }
}
