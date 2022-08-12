using Hackney.Core.Testing.DynamoDb;
using AutoFixture;
using System;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;
using Amazon.SimpleNotificationService;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.Tests.V1.E2ETests.Fixtures
{
    public class AssetsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        public readonly IDynamoDbFixture _dbFixture;

        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;

        public AssetDb Asset { get; private set; }
        public Asset AssetRequest { get; private set; }
        public Guid AssetId { get; private set; }
        public string PropertyReference { get; set; }
        public string InvalidAssetId { get; private set; }
        public Asset ExistingAsset { get; private set; }

        public AssetsFixture(IDynamoDbFixture dbFixture, IAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _dbFixture = dbFixture;
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (Asset != null)
                    _dbFixture.DynamoDbContext.DeleteAsync(Asset).GetAwaiter().GetResult();

                _disposed = true;
            }
        }

        public void PrepareAssetObject()
        {
            var asset = _fixture.Build<Asset>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();
            asset.Id = Guid.NewGuid();

            AssetRequest = asset;
        }

        public void GivenAnAssetAlreadyExists()
        {
            var entity = _fixture.Build<AssetDb>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();

            _dbFixture.DynamoDbContext.SaveAsync<AssetDb>(entity).GetAwaiter().GetResult();
            entity.VersionNumber = 0;

            ExistingAsset = entity.ToDomain();
            Asset = entity;
            AssetId = entity.Id;
            PropertyReference = entity.AssetId;
        }

        public void GivenAnAssetThatDoesntExist()
        {
            AssetId = Guid.NewGuid();
            PropertyReference = _fixture.Create<string>();
        }

        public void GivenAnEmptyAssetId()
        {
            var asset = _fixture.Build<Asset>()
               .With(x => x.VersionNumber, (int?) null)
               .Create();
            asset.Id = Guid.Empty;

            AssetRequest = asset;
        }

        public void GivenAnInvalidAssetId()
        {
            InvalidAssetId = "12345667890";
        }
    }
}
