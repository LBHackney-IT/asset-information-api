using AutoFixture;
using Hackney.Shared.Asset.Boundary.Request;
using AssetInformationApi.V1.Factories;
using Hackney.Shared.Asset.Factories;
using Xunit;
using FluentAssertions;

namespace AssetInformationApi.Tests.V1.Factories
{
    public class RequestFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void EditAssetRequestCorrectlyMapsToItsDatabaseLayerEquivalent()
        {
            // arrange
            var editAssetRequest = _fixture.Create<EditAssetRequest>();

            // act
            var editAssetDatabase = editAssetRequest.ToDatabase();

            // assert
            editAssetDatabase.RootAsset.Should().Be(editAssetRequest.RootAsset);
            editAssetDatabase.ParentAssetIds.Should().Be(editAssetRequest.ParentAssetIds);
            editAssetDatabase.IsActive.Should().Be(editAssetRequest.IsActive);
            editAssetDatabase.AssetLocation.Should().Be(editAssetRequest.AssetLocation);
            editAssetDatabase.AssetManagement.Should().Be(editAssetRequest.AssetManagement);
            editAssetDatabase.AssetCharacteristics.Should().BeEquivalentTo(editAssetRequest.AssetCharacteristics.ToDatabase());
        }

        [Fact]
        public void EditAssetAddressRequestCorrectlyMapsToItsDatabaseLayerEquivalent()
        {
            // arrange
            var editAssetAddressRequest = _fixture.Create<EditAssetAddressRequest>();

            // act
            var editAssetAddressDatabase = editAssetAddressRequest.ToDatabase();

            // assert
            editAssetAddressDatabase.AssetAddress.Should().Be(editAssetAddressRequest.AssetAddress);

            editAssetAddressDatabase.RootAsset.Should().Be(editAssetAddressRequest.RootAsset);
            editAssetAddressDatabase.ParentAssetIds.Should().Be(editAssetAddressRequest.ParentAssetIds);
            editAssetAddressDatabase.IsActive.Should().Be(editAssetAddressRequest.IsActive);
            editAssetAddressDatabase.AssetLocation.Should().Be(editAssetAddressRequest.AssetLocation);
            editAssetAddressDatabase.AssetManagement.Should().Be(editAssetAddressRequest.AssetManagement);
            editAssetAddressDatabase.AssetCharacteristics.Should().BeEquivalentTo(editAssetAddressRequest.AssetCharacteristics.ToDatabase());
        }
    }
}
