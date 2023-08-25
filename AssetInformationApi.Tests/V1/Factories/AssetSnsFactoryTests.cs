using AssetInformationApi.V1.Infrastructure;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using Xunit;
using AssetInformationApi.V1.Factories;

namespace AssetInformationApi.Tests.V1.Factories
{
    public class AssetSnsFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();

        private UpdateEntityResult<AssetDb> CreateUpdateEntityResult(AssetDb assetDb)
        {
            return _fixture.Build<UpdateEntityResult<AssetDb>>()
                                       .With(x => x.UpdatedEntity, assetDb)
                                       .With(x => x.OldValues, new Dictionary<string, object> { { "assetType", "Block" } })
                                       .With(x => x.NewValues, new Dictionary<string, object> { { "assetType", "Dwelling" } })
                                       .Create();
        }

        [Fact]
        public void CreateAssetTestCreatesSnsEvent()
        {
            var asset = _fixture.Create<Asset>();
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = asset };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new AssetSnsFactory();
            var result = factory.CreateAsset(asset, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, 100);
            result.EntityId.Should().Be(asset.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(CreateAssetEventConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(CreateAssetEventConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(CreateAssetEventConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(CreateAssetEventConstants.V1_VERSION);
        }

        [Fact]
        public void UpdateTestCreatesSnsEvent()
        {
            var assetDb = _fixture.Create<AssetDb>();

            var updateResult = CreateUpdateEntityResult(assetDb);
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = updateResult.NewValues, OldData = updateResult.OldValues };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new AssetSnsFactory();
            var result = factory.UpdateAsset(updateResult, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, 100);
            result.EntityId.Should().Be(assetDb.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(UpdateAssetConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(UpdateAssetConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(UpdateAssetConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(UpdateAssetConstants.V1_VERSION);
        }

        [Fact]
        public void AddRepairsContractsToAssetEvent()
        {
            var addRepairsContractsToNewAssetObject = _fixture.Create<AddRepairsContractsToNewAssetObject>();
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = addRepairsContractsToNewAssetObject };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new AssetSnsFactory();
            var result = factory.AddRepairsContractsToNewAsset(addRepairsContractsToNewAssetObject, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, 100);
            result.EntityId.Should().Be(addRepairsContractsToNewAssetObject.EntityId);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(AddRepairsContractsToAssetEventConstants.EVENT_TYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(AddRepairsContractsToAssetEventConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(AddRepairsContractsToAssetEventConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(AddRepairsContractsToAssetEventConstants.V1_VERSION);
        }
    }
}
