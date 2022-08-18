using FluentValidation;
using System;

namespace AssetInformationApi.V1.Boundary.Request.Validation
{
    public class AddAssetRequestValidator : AbstractValidator<AddAssetRequest>
    {
        public AddAssetRequestValidator()
        {
            RuleFor(x => x.Id).NotNull()
                              .NotEqual(Guid.Empty);
            RuleFor(x => x.AssetAddress).NotNull();
            RuleFor(x => x.AssetAddress.AddressLine1).NotNull()
                                 .NotEmpty();
            RuleFor(x => x.AssetAddress.PostCode).NotNull()
                                 .NotEmpty();
        }
    }
}
