using AutoFixture;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.Tests.V1.Gateways
{
    [Collection("AppTest collection")]
    public class DynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDbFixture _dbFixture;
        private readonly Mock<ILogger<DynamoDbGateway>> _logger;
        private readonly DynamoDbGateway _classUnderTest;

        public DynamoDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _logger = new Mock<ILogger<DynamoDbGateway>>();
            _classUnderTest = new DynamoDbGateway(_dbFixture.DynamoDbContext, _logger.Object);
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
                _disposed = true;
            }
        }

        private static GetAssetByIdRequest ConstructRequest(Guid? id = null)
        {
            return new GetAssetByIdRequest() { Id = id ?? Guid.NewGuid() };
        }

        private async Task InsertDataIntoDynamoDB(AssetDb entity)
        {
            await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetAssetByIdReturnsNullIfEntityDoesntExist()
        {
            var request = ConstructRequest();
            var response = await _classUnderTest.GetAssetByIdAsync(request).ConfigureAwait(false);

            response.Should().BeNull();
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {request.Id}", Times.Once());
        }

        [Fact]
        public async Task GetAssetByIdReturnsTheEntityIfItExists()
        {
            var entity = _fixture.Create<Asset>();
            entity.Tenure.StartOfTenureDate = DateTime.UtcNow;
            entity.Tenure.EndOfTenureDate = DateTime.UtcNow;

            await InsertDataIntoDynamoDB(entity.ToDatabase()).ConfigureAwait(false);

            var request = ConstructRequest(entity.Id);
            var response = await _classUnderTest.GetAssetByIdAsync(request).ConfigureAwait(false);

            response.Should().BeEquivalentTo(entity);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {request.Id}", Times.Once());
        }

        [Fact]
        public async Task GetAssetByAssetIdWhenEntityDoesntExistReturnsNull()
        {
            // Arrange
            var query = new GetAssetByAssetIdRequest
            {
                AssetId = _fixture.Create<string>()
            };

            // Act
            var response = await _classUnderTest.GetAssetByAssetId(query).ConfigureAwait(false);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetByAssetIdWhenEntityExistsReturnsEntity()
        {
            // Arrange
            var entity = _fixture.Create<AssetDb>();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new GetAssetByAssetIdRequest
            {
                AssetId = entity.AssetId
            };

            // Act
            var response = await _classUnderTest.GetAssetByAssetId(query).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(entity.ToDomain());

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for AssetId {query.AssetId}", Times.Once());
        }
    }
}
