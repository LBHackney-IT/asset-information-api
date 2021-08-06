using FluentValidation;
using System;

namespace AssetInformationApi.V1.Boundary.Request.Validation
{
    public class GetAssetByIdRequestValidator : AbstractValidator<GetAssetByIdRequest>
    {
        public GetAssetByIdRequestValidator()
        {
            RuleFor(x => x.Id).NotNull()
                              .NotEqual(Guid.Empty);
        }
    }
}
