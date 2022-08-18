using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Request.Validation;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace AssetInformationApi.Tests.V1.Boundary.Request.Validation
{
    public class AddAssetRequestValidatorTests
    {
        private readonly AddAssetRequestValidator _sut;

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
    }
}
