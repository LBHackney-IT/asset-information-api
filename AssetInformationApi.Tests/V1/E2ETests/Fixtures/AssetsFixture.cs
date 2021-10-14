using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using Hackney.Shared.Asset.Infrastructure;
using System;

namespace AssetInformationApi.Tests.V1.E2ETests.Fixtures
{
    public class AssetsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDBContext _dbContext;

        public AssetDb Asset { get; private set; }
        public Guid AssetId { get; private set; }
        public string InvalidAssetId { get; private set; }

        public AssetsFixture(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
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
                    _dbContext.DeleteAsync(Asset).GetAwaiter().GetResult();

                _disposed = true;
            }
        }

        public void GivenAnAssetAlreadyExists()
        {
            Asset = _fixture.Create<AssetDb>();
            AssetId = Asset.Id;
            _dbContext.SaveAsync(Asset).GetAwaiter().GetResult();
        }

        public void GivenAnAssetThatDoesntExist()
        {
            AssetId = Guid.NewGuid();
        }

        public void GivenAnInvalidAssetId()
        {
            InvalidAssetId = "12345667890";
        }
    }
}
