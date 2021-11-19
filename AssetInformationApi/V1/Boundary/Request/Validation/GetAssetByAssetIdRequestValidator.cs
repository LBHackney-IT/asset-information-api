using FluentValidation;
using Hackney.Core.Validation;
using Hackney.Shared.Tenure.Boundary.Requests.Validation;
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

            RuleFor(x => x.AssetId).NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure);
        }
    }
}
