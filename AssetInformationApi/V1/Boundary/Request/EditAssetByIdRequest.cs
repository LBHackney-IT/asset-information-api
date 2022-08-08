using Microsoft.AspNetCore.Mvc;
using System;

namespace AssetInformationApi.V1.Boundary.Request
{
    public class EditAssetByIdRequest
    {
        [FromRoute(Name = "id")]
        public Guid Id { get; set; }
    }
}
