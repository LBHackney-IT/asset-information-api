using FluentAssertions;
using Hackney.Core.Testing.Shared.E2E;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Asset.Domain;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Hackney.Core.Testing.DynamoDb;
using System;
using Hackney.Shared.Asset.Factories;

namespace AssetInformationApi.Tests.V1.E2ETests.Steps
{
    public class AddNewAssetSteps : BaseSteps
    {
        private readonly IDynamoDbFixture _dbFixture;

        public AddNewAssetSteps(HttpClient httpClient, IDynamoDbFixture dbFixture) : base(httpClient)
        {
            _dbFixture = dbFixture;
        }

        public async Task WhenTheAddAssetApiIsCalled(Asset asset)
        {
            var route = $"/api/v1/assets/add";

            if (asset.Id == Guid.Empty)
                _lastResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            else
                _lastResponse = await _httpClient.PostAsync(route, asset, new JsonMediaTypeFormatter()).ConfigureAwait(false);
        }

        public async Task ThenTheAssetDetailsAreReturned(Asset request)
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiProcess = JsonConvert.DeserializeObject<Asset>(responseContent);

            apiProcess.Id.Should().NotBeEmpty();
            var dbRecord = await _dbFixture.DynamoDbContext.LoadAsync<AssetDb>(apiProcess.Id).ConfigureAwait(false);

            dbRecord.Id.Should().Be(request.Id);
            dbRecord.AssetId.Should().Be(request.AssetId);
            dbRecord.AssetType.Should().Be(request.AssetType);
            dbRecord.AssetLocation.FloorNo.Should().Be(request.AssetLocation.FloorNo);
            dbRecord.AssetManagement.Agent.Should().Be(request.AssetManagement.Agent);
            dbRecord.AssetCharacteristics.BathroomFloor.Should().Be(request.AssetCharacteristics.BathroomFloor);
            // Cleanup
            await _dbFixture.DynamoDbContext.DeleteAsync<AssetDb>(dbRecord.Id).ConfigureAwait(false);
        }

        public void ThenBadRequestIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public void ThenNotFoundIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
