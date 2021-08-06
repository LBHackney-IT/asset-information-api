using AssetInformationApi.V1.Domain;
using AssetInformationApi.V1.Factories;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AssetInformationApi.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CanMapANullAssetToAResponseObject()
        {
            Asset domain = null;
            var response = domain.ToResponse();

            response.Should().BeNull();
        }

        [Fact]
        public void CanMapAnAssetToAResponseObject()
        {
            var domain = _fixture.Create<Asset>();
            var response = domain.ToResponse();
            domain.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void CanMapANullAssetTenureToAResponseObject()
        {
            AssetTenure domain = null;
            var response = domain.ToResponse();

            response.Should().BeNull();
        }

        [Fact]
        public void CanMapAnAssetTenureToAResponseObject()
        {
            var domain = _fixture.Create<AssetTenure>();
            var response = domain.ToResponse();
            domain.Should().BeEquivalentTo(response);
        }
    }
}
