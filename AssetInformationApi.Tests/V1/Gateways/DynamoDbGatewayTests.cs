using Amazon.DynamoDBv2.DataModel;
using AssetInformationApi.V1.Boundary.Request;
using AssetInformationApi.V1.Gateways;
using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AssetInformationApi.Tests.V1.Gateways
{
    [Collection("DynamoDb collection")]
    public class DynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDBContext _dynamoDb;
        private readonly Mock<ILogger<DynamoDbGateway>> _logger;
        private readonly DynamoDbGateway _classUnderTest;
        private readonly List<Action> _cleanup = new List<Action>();

        public DynamoDbGatewayTests(DynamoDbIntegrationTests<Startup> dbTestFixture)
        {
            _dynamoDb = dbTestFixture.DynamoDbContext;
            _logger = new Mock<ILogger<DynamoDbGateway>>();
            _classUnderTest = new DynamoDbGateway(_dynamoDb, _logger.Object);
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
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }

        private static GetAssetByIdRequest ConstructRequest(Guid? id = null)
        {
            return new GetAssetByIdRequest() { Id = id ?? Guid.NewGuid() };
        }

        private async Task InsertDataIntoDynamoDB(AssetDb entity)
        {
            await _dynamoDb.SaveAsync(entity).ConfigureAwait(false);
            _cleanup.Add(async () => await _dynamoDb.DeleteAsync(entity).ConfigureAwait(false));
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
    }
}
