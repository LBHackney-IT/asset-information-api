using Amazon.SimpleNotificationService;
using AutoFixture;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Shared.Asset.Boundary.Request;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using System;

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

        public EditAssetAddressRequest EditAssetAddress { get; private set; }

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
                    _dbFixture.DynamoDbContext.DeleteAsync<AssetDb>(Asset.Id).GetAwaiter().GetResult();

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

        public void PrepareAssetObjectWithAssetId()
        {
            var asset = _fixture.Build<Asset>()
                .With(x => x.VersionNumber, (int?) null)
                .With(x => x.AssetId, "12345678910")
                //stop AssetManagement validation from interfering since that property is not relevant to the test
                .With(x => x.AssetManagement, (AssetManagement) null)
                .Create();
            asset.Id = Guid.NewGuid();

            AssetRequest = asset;
        }

        public void CreateEditAssetObject()
        {
            var asset = _fixture.Build<EditAssetRequest>()
                .Create();

            EditAsset = asset;
        }

        public void CreateEditAssetAddressObject()
        {
            var asset = _fixture.Build<EditAssetAddressRequest>()
                .Create();

            EditAssetAddress = asset;
            EditAsset = asset;
        }

        public void GivenAnAssetAlreadyExists()
        {
            var entity = _fixture.Build<AssetDb>()
                .Without(x => x.Tenure)
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
