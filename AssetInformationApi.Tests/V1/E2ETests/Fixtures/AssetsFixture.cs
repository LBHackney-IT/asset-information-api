using Hackney.Core.Testing.DynamoDb;
using AutoFixture;
using System;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;

namespace AssetInformationApi.Tests.V1.E2ETests.Fixtures
{
    public class AssetsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDbFixture _dbFixture;

        public AssetDb Asset { get; private set; }
        public Asset AssetRequest { get; private set; }
        public Guid AssetId { get; private set; }
        public string PropertyReference { get; set; }
        public string InvalidAssetId { get; private set; }

        public AssetsFixture(IDynamoDbFixture dbFixture)
        {
            _dbFixture = dbFixture;
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
            Asset = _fixture.Build<AssetDb>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();

            AssetId = Asset.Id;
            PropertyReference = Asset.AssetId;

            _dbFixture.DynamoDbContext.SaveAsync(Asset).GetAwaiter().GetResult();
        }

        public void GivenAnAssetThatDoesntExist()
        {
            AssetId = Guid.NewGuid();
            PropertyReference = _fixture.Create<string>();
        }

        public void GivenAnInvalidAssetId()
        {
            var asset = _fixture.Build<Asset>()
               .With(x => x.VersionNumber, (int?) null)
               .Create();
            asset.Id = Guid.Empty;

            AssetRequest = asset;
        }
    }
}
