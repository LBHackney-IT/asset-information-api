using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Request.Validation;
using AutoFixture;
using FluentAssertions;
using FluentValidation.TestHelper;
using System;
using System.Linq;
using Xunit;

namespace AssetInformationApi.Tests.V1.Boundary.Request.Validation
{
#nullable enable
    public class AddAssetRequestValidatorTests
    {
        private readonly AddAssetRequestValidator _sut;
        private readonly Fixture _fixture = new();

        public AddAssetRequestValidatorTests()
        {
            _sut = new AddAssetRequestValidator();
        }

        [Fact]
        public void RequestShouldErrorForEmptyId()
        {
            var model = new AddAssetRequest() { Id = Guid.Empty, AssetAddress = new Hackney.Shared.Asset.Domain.AssetAddress() };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void RequestShouldErrorForEmptyAddress()
        {
            var model = new AddAssetRequest() { Id = Guid.NewGuid(), AssetAddress = new Hackney.Shared.Asset.Domain.AssetAddress() };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.AssetAddress.AddressLine1);
            result.ShouldHaveValidationErrorFor(x => x.AssetAddress.PostCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void RequestShouldNotErrorWhenPostcodeIsEmptyOrNullForTemporarysAccommodationAsset(string? postcode)
        {
            var assetmanagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = true
            };

            var assetAddress = _fixture
                .Build<Hackney.Shared.Asset.Domain.AssetAddress>()
                .With(x => x.PostCode, postcode)
                .Create();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetmanagement
            };

            var result = _sut.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.AssetAddress.PostCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void RequestShouldErrorWhenPostCodeIsNullOrEmptyAndAssetManagementIsNull(string? postcode)
        {
            var assetAddress = _fixture
                .Build<Hackney.Shared.Asset.Domain.AssetAddress>()
                .With(x => x.PostCode, postcode)
                .Create();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = null
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetAddress.PostCode);
        }

        #region AssetManagement
        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public void RequestShouldErrorWhenIsTemporaryAccommodationBlockIsTrueAndIsTemporaryAccomodationIsnullOrFalse(bool? isTemporaryAccommodation)
        {
            var assetManagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = isTemporaryAccommodation,
                IsTemporaryAccommodationBlock = true,
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetManagement
            };

            var expectedErrorMessage = "IsTemporaryAccomodation must be true when IsTemporaryAccommodationBlock is set to true";

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetManagement.IsTemporaryAccomodation);
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessage).Should().BeTrue();
        }

        [Fact]
        public void RequestShouldErrorWhenIsTemporaryAccommodationBlockIsTrueAndTemporaryAccommodationParentAssetIdIsNotNull()
        {
            var assetManagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = true,
                IsTemporaryAccommodationBlock = true,
                TemporaryAccommodationParentAssetId = Guid.NewGuid(),
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetManagement
            };

            var expectedErrorMessage = "Temporary accommodation block cannot have TemporaryAccommodationParentAssetId";

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetManagement.TemporaryAccommodationParentAssetId);
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessage).Should().BeTrue();
        }

        [Fact]
        public void RequestShouldErrorWhenIsPartOfTemporaryAccommodationBlockIsTrueAndTemporaryAccommodationParentAssetIdIsNull()
        {
            var assetmanagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = true,
                IsTemporaryAccommodationBlock = false,
                IsPartOfTemporaryAccommodationBlock = true,
                TemporaryAccommodationParentAssetId = null,
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetmanagement,
            };

            var expectedErrorMessage = "TemporaryAccommodationParentAssetId cannot be null when IsPartOfTemporaryAccommodationBlock is true";

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetManagement.TemporaryAccommodationParentAssetId);
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessage).Should().BeTrue();
        }

        [Fact]
        public void RequestShouldErrorWhenIsPartOfTemporaryAccommodationBlockIsTrueAndIsTemporaryAccommodationBlockIsTrue()
        {
            var assetManagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = true,
                IsTemporaryAccommodationBlock = true,
                IsPartOfTemporaryAccommodationBlock = true,
                TemporaryAccommodationParentAssetId = null,
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetManagement,
            };

            var expectedErrorMessage = "IsPartOfTemporaryAccommodationBlock cannot be true when IsTemporaryAccommodationBlock is true";

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetManagement.IsPartOfTemporaryAccommodationBlock);
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessage).Should().BeTrue();
        }

        [Fact]
        public void RequestShouldNotHaveErrorsWhenIsTemporaryAccommodationBlockIsTrueAndIsPartOfTemporaryAccommodationBlockIsNull()
        {

            var assetManagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = true,
                IsTemporaryAccommodationBlock = true,
                TemporaryAccommodationParentAssetId = null,
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetManagement,
            };

            var result = _sut.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.AssetManagement.IsPartOfTemporaryAccommodationBlock);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public void RequestShouldErrorWhenTASpecificPropertiesAreSetForNonTAAsset(bool? isTemporaryAccommodation)
        {
            var assetManagement = new Hackney.Shared.Asset.Domain.AssetManagement()
            {
                IsTemporaryAccomodation = isTemporaryAccommodation,
                IsTemporaryAccommodationBlock = true,
                IsPartOfTemporaryAccommodationBlock = true,
                TemporaryAccommodationParentAssetId = Guid.NewGuid(),
            };

            var assetAddress = _fixture.Create<Hackney.Shared.Asset.Domain.AssetAddress>();

            var model = new AddAssetRequest()
            {
                Id = Guid.NewGuid(),
                AssetAddress = assetAddress,
                AssetManagement = assetManagement,
            };

            var expectedErrorMessageForIsTemporaryAccommodationBlock = "IsTemporaryAccommodationBlock cannot be true when IsTemporaryAccomodation is false";
            var expectedErrorMessageForIsPartOfTemporaryAccommodationBlock = "IsPartOfTemporaryAccommodationBlock cannot be true when IsTemporaryAccomodation is false";
            var expectedErrorMessageForTemporaryAccommodationParentAssetId = "TemporaryAccommodationParentAssetId must be null when IsTemporaryAccomodation is false";

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssetManagement.IsTemporaryAccommodationBlock);
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessageForIsTemporaryAccommodationBlock).Should().BeTrue();
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessageForIsPartOfTemporaryAccommodationBlock).Should().BeTrue();
            result.Errors.Any(x => x.ErrorMessage == expectedErrorMessageForTemporaryAccommodationParentAssetId).Should().BeTrue();
        }
        #endregion
    }
}
