using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Boundary.Request.Validation;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace AssetInformationApi.Tests.V1.Boundary.Request.Validation
{
    public class GetAssetByIdRequestValidatorTests
    {
        private readonly GetAssetByIdRequestValidator _sut;

        public GetAssetByIdRequestValidatorTests()
        {
            _sut = new GetAssetByIdRequestValidator();
        }

        [Fact]
        public void RequestShouldErrorWithNullTargetId()
        {
            var query = new GetAssetByIdRequest();
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void RequestShouldErrorWithEmptyTargetId()
        {
            var query = new GetAssetByIdRequest() { Id = Guid.Empty };
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
