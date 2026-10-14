using AssetInformationApi.Versioning;
using FluentAssertions;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Xunit;

namespace AssetInformationApi.Tests.Versioning
{
    public class ApiVersionDescriptionExtensionsTests
    {
        [Fact]
        public void GetFormattedApiVersionTest()
        {
            var version = new ApiVersion(1, 1);
            ApiVersionDescription sut = new ApiVersionDescription(version, null);
            sut.GetFormattedApiVersion().Should().Be($"v{version.ToString()}");
        }
    }
}
