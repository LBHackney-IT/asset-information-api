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
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using AssetInformationApi.V1.Infrastructure;
using Amazon.DynamoDBv2.DataModel;
using AssetInformationApi.V1.Infrastructure.Exceptions;
using System.Collections.Generic;
using Hackney.Shared.Asset.Domain;
using Force.DeepCloner;
using Hackney.Shared.Asset.Boundary.Request;
using AssetInformationApi.V1.Factories;


namespace AssetInformationApi.Tests.V1.Gateways
{
    [Collection("AppTest collection")]
    public class DynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDbFixture _dbFixture;
        private readonly Mock<ILogger<AssetGateway>> _logger;
        private AssetGateway _classUnderTest;
        private readonly Mock<IEntityUpdater> _updater;

        private const string RequestBody = "{ \"areaId\": \"6c805141-0e27-46b1-86ee-5d33c638ef24\", \"patchId\": \"d2c3a31f-636a-4875-81ce-24a7f5362cb9\"}";
        public DynamoDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _logger = new Mock<ILogger<AssetGateway>>();
            _updater = new Mock<IEntityUpdater>();
            _classUnderTest = new AssetGateway(_dbFixture.DynamoDbContext, _logger.Object, _updater.Object);
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

        private Asset ConstructAsset()
        {
            return _fixture.Build<Asset>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();
        }

        private EditAssetByIdRequest ConstructQuery(Guid id)
        {
            return new EditAssetByIdRequest() { Id = id };
        }

        private EditPropertyPatchRequest EditPropertyPatchRequest()
        {
            var areaId = "6c805141-0e27-46b1-86ee-5d33c638ef24";
            var patchId = "d2c3a31f-636a-4875-81ce-24a7f5362cb9";
            return _fixture.Build<EditPropertyPatchRequest>()
                           .With(x => x.AreaId, Guid.Parse(areaId))
                           .With(x => x.PatchId, Guid.Parse(patchId))
                           .Create();
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
            var entity = _fixture.Build<AssetDb>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();

            entity.Tenure.StartOfTenureDate = DateTime.UtcNow;
            entity.Tenure.EndOfTenureDate = DateTime.UtcNow;

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

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
            var entity = _fixture.Build<AssetDb>()
                .With(x => x.VersionNumber, (int?) null)
                .Create();

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

        [Fact]
        public async Task AddAssetToDatabaseAndReturnEntity()
        {
            // Arrange
            var entity = _fixture.Build<AssetDb>()
                .With(x => x.VersionNumber, (int?) null)
                .With(x => x.AssetId, (string) null)
                .Create();

            var query = new GetAssetByIdRequest()
            {
                Id = entity.Id
            };

            // Act
            var response = await _classUnderTest.AddAsset(entity).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(entity.ToDomain());

            await _dbFixture.DynamoDbContext.DeleteAsync<AssetDb>(query.Id).ConfigureAwait(false);
        }

        [Fact]
        public async Task EditPropertyPatchSuccessfulUpdates()
        {
            // Arrange
            var asset = ConstructAsset();
            var assetDb = asset.ToDatabase();
            await _dbFixture.SaveEntityAsync(assetDb).ConfigureAwait(false);

            var constructRequest = EditPropertyPatchRequest();

            var updatedAsset = asset.DeepClone();
            updatedAsset.AreaId = constructRequest.AreaId;
            updatedAsset.PatchId = constructRequest.PatchId;
            updatedAsset.VersionNumber = 0;
            _updater.Setup(x => x.UpdateEntity(It.IsAny<AssetDb>(), It.IsAny<string>(), It.IsAny<EditPropertyPatchDatabase>()))
                        .Returns(new UpdateEntityResult<AssetDb>()
                        {
                            UpdatedEntity = updatedAsset.ToDatabase(),
                            OldValues = new Dictionary<string, object>
                            {
                                { "areaId", asset.AreaId },
                                { "patchId", asset.PatchId },
                            },
                            NewValues = new Dictionary<string, object>
                            {
                                { "areaId", asset.AreaId },
                                { "patchId", asset.PatchId },
                            }
                        });

            //Act
            var response = await _classUnderTest.EditAssetDetails(asset.Id, constructRequest, RequestBody, 0).ConfigureAwait(false);

            //Assert
            var load = await _dbFixture.DynamoDbContext.LoadAsync<AssetDb>(asset.Id).ConfigureAwait(false);

            // Changed
            load.AreaId.Should().Be(updatedAsset.AreaId);
            load.PatchId.Should().Be(updatedAsset.PatchId);


            // Not changed
            load.Should().BeEquivalentTo(updatedAsset, options => options.Excluding(x => x.AreaId)
                                                                         .Excluding(x => x.PatchId)
                                                                         .Excluding(x => x.Tenure)
                                                                         .Excluding(x => x.VersionNumber));

            var expectedVersionNumber = 1;
            load.VersionNumber.Should().Be(expectedVersionNumber);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync to update id {asset.Id}", Times.Once());

        }

        [Theory]
        [InlineData(null)]
        [InlineData(5)]
        public async Task EditPropertyPatchThrowsExceptionOnVersionConflict(int? ifMatch)
        {
            // Arrange
            var asset = ConstructAsset();
            var assetDb = asset.ToDatabase();
            await _dbFixture.SaveEntityAsync(assetDb).ConfigureAwait(false);

            var constructRequest = EditPropertyPatchRequest();

            //Act
            Func<Task<UpdateEntityResult<AssetDb>>> func = async () => await _classUnderTest.EditAssetDetails(asset.Id, constructRequest, RequestBody, ifMatch)
                                                                                                   .ConfigureAwait(false);

            // Assert
            func.Should().Throw<VersionNumberConflictException>()
                         .Where(x => (x.IncomingVersionNumber == ifMatch) && (x.ExpectedVersionNumber == 0));
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync to update id {asset.Id}", Times.Never());
        }

        [Fact]
        public async Task EditPropertyPatchByIdReturnsNullIfEntityDoesntExist()
        {
            // Act
            var id = Guid.NewGuid();
            var query = ConstructQuery(id);
            var constructRequest = EditPropertyPatchRequest();

            var response = await _classUnderTest.EditAssetDetails(query.Id, constructRequest, RequestBody, 0).ConfigureAwait(false);

            // Assert
            response.Should().BeNull();
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync to update id {query.Id}", Times.Never());
        }

        [Fact]
        public void EditPropertyPatchByIdExceptionThrow()
        {
            // Arrange
            var mockDynamoDb = new Mock<IDynamoDBContext>();
            _classUnderTest = new AssetGateway(mockDynamoDb.Object, _logger.Object, _updater.Object);
            var id = Guid.NewGuid();
            var query = ConstructQuery(id);
            var constructRequest = EditPropertyPatchRequest();
            var exception = new ApplicationException("Test exception");
            mockDynamoDb.Setup(x => x.LoadAsync<AssetDb>(id, default))
                        .ThrowsAsync(exception);

            // Act
            Func<Task<UpdateEntityResult<AssetDb>>> func = async () => await _classUnderTest.EditAssetDetails(query.Id, constructRequest, RequestBody, 0)
                                                                                                   .ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
            mockDynamoDb.Verify(x => x.LoadAsync<AssetDb>(id, default), Times.Once);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync to update id {query.Id}", Times.Never());
        }
    }
}
