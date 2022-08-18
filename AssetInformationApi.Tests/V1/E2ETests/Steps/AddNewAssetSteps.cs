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
using Hackney.Core.JWT;
using System.Text;
using System.Net.Http.Headers;
using Hackney.Core.Testing.Sns;
using Hackney.Core.Sns;
using AssetInformationApi.V1.Infrastructure;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AssetInformationApi.Tests.V1.E2ETests.Steps
{
    public class AddNewAssetSteps : BaseSteps
    {
        private readonly IDynamoDbFixture _dbFixture;

        public AddNewAssetSteps(HttpClient httpClient, IDynamoDbFixture dbFixture) : base(httpClient)
        {
            _dbFixture = dbFixture;
        }

        /// <summary>
        /// You can use jwt.io to decode the token - it is the same one we'd use on dev, etc. 
        /// </summary>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CallAPI(Asset asset)
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";
            var uri = new Uri($"api/v1/assets/add", UriKind.Relative);

            using (var message = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(asset), Encoding.UTF8, "application/json");
                message.Method = HttpMethod.Post;
                message.Headers.Add("Authorization", token);

                _httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return await _httpClient.SendAsync(message).ConfigureAwait(false);
            }
        }

        public async Task WhenTheAddAssetApiIsCalled(Asset asset)
        {
            var route = $"/api/v1/assets/add";
            //var token = new Token();
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
        public async Task ThenAssetDetailsAreReturnedAndTheAssetCreatedEventIsRaised(Asset request, ISnsFixture snsFixture)
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

            Action<EntityEventSns> verifyFunc = (actual) =>
            {
                actual.CorrelationId.Should().NotBeEmpty();
                actual.DateTime.Should().BeCloseTo(DateTime.UtcNow, 500000);
                actual.EntityId.Should().Be(dbRecord.Id);

                var expected = dbRecord.ToDomain();
                var actualNewData = JsonSerializer.Deserialize<Asset>(actual.EventData.NewData.ToString(), CreateJsonOptions());
                actualNewData.Should().BeEquivalentTo(expected);
                actual.EventData.OldData.Should().BeNull();

                actual.EventType.Should().Be(CreateAssetEventConstants.EVENTTYPE);
                actual.Id.Should().NotBeEmpty();
                actual.SourceDomain.Should().Be(CreateAssetEventConstants.SOURCE_DOMAIN);
                actual.SourceSystem.Should().Be(CreateAssetEventConstants.SOURCE_SYSTEM);
                actual.User.Email.Should().Be("e2e-testing@development.com");
                actual.User.Name.Should().Be("Tester");
                actual.Version.Should().Be(CreateAssetEventConstants.V1_VERSION);
            };

            var snsVerifer = snsFixture.GetSnsEventVerifier<EntityEventSns>();
            var snsResult = await snsVerifer.VerifySnsEventRaised(verifyFunc).ConfigureAwait(false);
            if (!snsResult && snsVerifer.LastException != null)
                throw snsVerifer.LastException;

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

        public async Task WhenTheAddAssetApiIsCalledWithAToken(Asset asset)
        {
            _lastResponse = await CallAPI(asset).ConfigureAwait(false);
        }
    }
}
