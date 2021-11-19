using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Request.Validation;
using FluentValidation.TestHelper;
using Hackney.Shared.Tenure.Boundary.Requests.Validation;
using System;
using Xunit;

namespace AssetInformationApi.Tests.V1.Boundary.Request.Validation
{
    public class GetAssetByAssetIsRequestValidatorTests
    {
        private readonly GetAssetByAssetIdRequestValidator _sut;
        private const string StringWithTags = "Some string with <tag> in it.";

        public GetAssetByAssetIsRequestValidatorTests()
        {
            _sut = new GetAssetByAssetIdRequestValidator();
        }

        [Fact]
        public void WhenNullHasError()
        {
            // Arrange
            var model = new GetAssetByAssetIdRequest();

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AssetId);
        }

        [Fact]
        public void WhenEmptyHasError()
        {
            // Arrange
            var model = new GetAssetByAssetIdRequest
            {
                AssetId = ""
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AssetId);
        }

        [Fact]
        public void ShouldErrorWhenContainsTags()
        {
            // Arrange
            var model = new GetAssetByAssetIdRequest
            {
                AssetId = StringWithTags
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AssetId)
                .WithErrorCode(ErrorCodes.XssCheckFailure);
        }
    }
}
