using Hackney.Core.Testing.DynamoDb;
using AutoFixture;
using System;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;
using Amazon.SimpleNotificationService;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Hackney.Shared.Asset.Factories;
using AssetInformationApi.V1.Boundary.Request;

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

        public EditAssetRequest EditAsset { get; private set; }

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

        public void CreateEditAssetObject()
        {
            var asset = _fixture.Build<EditAssetRequest>()
                .With(x => x.Id, AssetId)
                .With(x => x.VersionNumber, (int?) null)
                .Create();

            EditAsset = asset;
        }

        public void GivenAnAssetAlreadyExists()
        {
            var entity = _fixture.Build<AssetDb>()
                .Without(x => x.Tenure)
                .With(x => x.AssetAddress, _fixture.Build<AssetAddress>()
                    .With(a => a.Uprn, "700123")
                    .With(a => a.AddressLine1, "AddressLine1")
                    .With(a => a.AddressLine2, "AddressLine2")
                    .With(a => a.AddressLine3, "AddressLine3")
                    .With(a => a.AddressLine4, "AddressLine4")
                    .With(a => a.PostCode, "N99EE")
                    .With(a => a.PostPreamble, "PostPreamble")
                    .Create())
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
