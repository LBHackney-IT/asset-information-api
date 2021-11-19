using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.Boundary.Request.Validation
{
    public class GetAssetByAssetIdRequestValidator : AbstractValidator<GetAssetByAssetIdRequest>
    {
        public GetAssetByAssetIdRequestValidator()
        {
            RuleFor(x => x.AssetId)
                .NotNull()
                .NotEmpty();
        }
    }
}
