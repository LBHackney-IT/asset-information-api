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
            RuleFor(x => x.AssetAddress.PostCode)
                .NotNull()
                .NotEmpty()
                .When(x => x.AssetManagement.IsTemporaryAccomodation != true);

            When(x => x.AssetManagement != null, () =>
            {
                RuleFor(x => x.AssetManagement.IsTemporaryAccomodation)
                    .Must(x => x == true)
                    .When(x => x.AssetManagement.IsTemporaryAccommodationBlock == true)
                    .WithMessage("IsTemporaryAccomodation must be true when IsTemporaryAccommodationBlock is set to true");

                RuleFor(x => x.AssetManagement.TemporaryAccommodationParentAssetId)
                    .Null()
                    .When(x => x.AssetManagement.IsTemporaryAccommodationBlock == true)
                    .WithMessage("Temporary accommodation block cannot have TemporaryAccommodationParentAssetId");
            });
        }
    }
}
