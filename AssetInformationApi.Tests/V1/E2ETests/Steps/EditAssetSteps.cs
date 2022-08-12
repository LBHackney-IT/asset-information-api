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
using Hackney.Core.Testing.Shared.E2E;
using System.Net.Http;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;
using System.Net;
using System.Net.Http.Formatting;
using FluentAssertions;
using Hackney.Shared.Asset.Infrastructure;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Linq;
using AssetInformationApi.Tests.V1.E2ETests.Fixtures;
using HousingSearchApi.V1.Boundary.Response;
using System.Collections.Generic;
using AssetInformationApi.V1.Boundary.Request;
using Microsoft.AspNetCore.Http;
using Amazon.DynamoDBv2.DataModel;

namespace AssetInformationApi.Tests.V1.E2ETests.Steps
{
    public class EditAssetSteps : BaseSteps
    {
        private readonly IDynamoDBContext _dbContext;

        public EditAssetSteps(HttpClient httpClient, IDynamoDBContext dbContext) : base(httpClient)
        {
            _dbContext = dbContext;
        }


        public async Task WhenEditAssetApiIsCalled(Guid id, object requestObject)
        {
            int? defaultIfMatch = 0;
            await WhenEditAssetApiIsCalled(id, requestObject, defaultIfMatch).ConfigureAwait(false);
        }

        public async Task WhenEditAssetApiIsCalled(Guid id, object requestObject, int? ifMatch)
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";

            // setup request
            var uri = new Uri($"api/v1/assets/{id}", UriKind.Relative);
            using (var message = new HttpRequestMessage(HttpMethod.Patch, uri))
            {

                message.Method = HttpMethod.Patch;
                message.Headers.Add("Authorization", token);
                message.Headers.TryAddWithoutValidation(HeaderConstants.IfMatch, $"\"{ifMatch?.ToString()}\"");

                var jsonSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new[] { new StringEnumConverter() }
                };
                var requestJson = JsonConvert.SerializeObject(requestObject, jsonSettings);
                message.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");


                // call request
                _httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _lastResponse = await _httpClient.SendAsync(message).ConfigureAwait(false);
            }
        }

        public void ThenBadRequestIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public void ThenNotFoundIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        public void ThenNoContentResponseReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        public async Task ThenTheValidationErrorsAreReturned(string errorMessageName)
        {
            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            JObject jo = JObject.Parse(responseContent);
            var errors = jo["errors"].Children();

            ShouldHaveErrorFor(errors, errorMessageName);
        }

        public async Task ThenConflictIsReturned(int? versionNumber)
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            var sentVersionNumberString = (versionNumber is null) ? "{null}" : versionNumber.ToString();
            responseContent.Should().Contain($"The version number supplied ({sentVersionNumberString}) does not match the current value on the entity (0).");
        }

        private static void ShouldHaveErrorFor(JEnumerable<JToken> errors, string propertyName, string errorCode = null)
        {
            var error = errors.FirstOrDefault(x => (x.Path.Split('.').Last().Trim('\'', ']')) == propertyName) as JProperty;
            error.Should().NotBeNull();
            if (!string.IsNullOrEmpty(errorCode))
                error.Value.ToString().Should().Contain(errorCode);
        }

        public async Task TheAssetHasntBeenUpdatedInTheDatabase(AssetsFixture assetFixture)
        {
            var databaseResponse = await _dbContext.LoadAsync<AssetDb>(assetFixture.AssetId).ConfigureAwait(false);

            databaseResponse.Id.Should().Be(assetFixture.ExistingAsset.Id);
            databaseResponse.AssetAddress.Should().Be(assetFixture.ExistingAsset.AssetAddress);
            databaseResponse.AssetCharacteristics.Should().Be(assetFixture.ExistingAsset.AssetCharacteristics);
            databaseResponse.AssetManagement.Should().Be(assetFixture.Asset.AssetManagement);
            databaseResponse.AssetType.Should().Be(assetFixture.ExistingAsset.AssetType);
            databaseResponse.ParentAssetIds.Should().Be(assetFixture.ExistingAsset.ParentAssetIds);
            databaseResponse.RootAsset.Should().Be(assetFixture.ExistingAsset.RootAsset);
            databaseResponse.AssetLocation.Should().Be(assetFixture.ExistingAsset.AssetLocation);
            databaseResponse.AssetId.Should().Be(assetFixture.ExistingAsset.AssetId);
            databaseResponse.Tenure.Should().Be(assetFixture.ExistingAsset.Tenure);
        }

        public async Task ThenEditAssetBadRequestIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseEntity = JsonSerializer.Deserialize<ErrorResponse>(responseContent, CreateJsonOptions());

            responseEntity.Should().BeOfType(typeof(ErrorResponse));

            responseEntity.Error.Should().Be(StatusCodes.Status400BadRequest);
        }

        public async Task TheAssetHasBeenUpdatedInTheDatabase(AssetsFixture assetFixture, EditAssetRequest requestObject)
        {
            var databaseResponse = await _dbContext.LoadAsync<AssetDb>(assetFixture.AssetId).ConfigureAwait(false);

            databaseResponse.Id.Should().Be(assetFixture.ExistingAsset.Id);
            databaseResponse.AssetAddress.Should().Be(requestObject.AssetAddress);
            databaseResponse.AssetCharacteristics.Should().Be(requestObject.AssetCharacteristics);
            databaseResponse.AssetManagement.Should().Be(requestObject.AssetManagement);
            databaseResponse.AssetType.Should().Be(requestObject.AssetType);
            databaseResponse.ParentAssetIds.Should().Be(requestObject.ParentAssetIds);
            databaseResponse.RootAsset.Should().Be(requestObject.RootAsset);
            databaseResponse.AssetLocation.Should().Be(requestObject.AssetLocation);
            databaseResponse.AssetId.Should().Be(requestObject.AssetId);
            databaseResponse.Tenure.Should().Be(requestObject.Tenure);
        }

        public async Task ThenTheAssetUpdatedEventIsRaised(AssetsFixture assetFixture, ISnsFixture snsFixture)
        {
            var dbRecord = await _dbContext.LoadAsync<AssetDb>(assetFixture.Asset.Id).ConfigureAwait(false);

            Action<EntityEventSns> verifyFunc = (actual) =>
            {
                actual.CorrelationId.Should().NotBeEmpty();
                actual.DateTime.Should().BeCloseTo(DateTime.UtcNow, 2000);
                actual.EntityId.Should().Be(dbRecord.Id);

                var expectedOldData = new Dictionary<string, object>
                {
                    { "assetId", assetFixture.Asset.AssetId },
                    { "rootAsset", assetFixture.Asset.RootAsset },
                    { "parentAssetIds", assetFixture.Asset.ParentAssetIds },
                    { "assetType", assetFixture.Asset.AssetType },

                    { "assetAddress", assetFixture.Asset.AssetAddress },
                    { "assetCharacteristics", assetFixture.Asset.AssetCharacteristics },
                    { "assetManagement", assetFixture.Asset.AssetManagement },
                    { "assetLocation", assetFixture.Asset.AssetLocation },
                    { "tenure", assetFixture.Asset.Tenure },
                };
                var expectedNewData = new Dictionary<string, object>
                {
                    { "assetId", dbRecord.AssetId },
                    { "rootAsset", dbRecord.RootAsset },
                    { "parentAssetIds", dbRecord.ParentAssetIds },
                    { "assetType", dbRecord.AssetType },

                    { "assetAddress", dbRecord.AssetAddress },
                    { "assetCharacteristics", dbRecord.AssetCharacteristics },
                    { "assetManagement", dbRecord.AssetManagement },
                    { "assetLocation", dbRecord.AssetLocation },
                    { "tenure", dbRecord.Tenure },
                };
                VerifyEventData(actual.EventData.OldData, expectedOldData);
                VerifyEventData(actual.EventData.NewData, expectedNewData);

                actual.EventType.Should().Be(UpdateAssetConstants.EVENTTYPE);
                actual.Id.Should().NotBeEmpty();
                actual.SourceDomain.Should().Be(UpdateAssetConstants.SOURCE_DOMAIN);
                actual.SourceSystem.Should().Be(UpdateAssetConstants.SOURCE_SYSTEM);
                actual.User.Email.Should().Be("e2e-testing@development.com");
                actual.User.Name.Should().Be("Tester");
                actual.Version.Should().Be(UpdateAssetConstants.V1_VERSION);
            };

            var snsVerifer = snsFixture.GetSnsEventVerifier<EntityEventSns>();
            var snsResult = await snsVerifer.VerifySnsEventRaised(verifyFunc).ConfigureAwait(false);
            if (!snsResult && snsVerifer.LastException != null)
                throw snsVerifer.LastException;
        }

        private void VerifyEventData(object eventDataJsonObj, Dictionary<string, object> expected)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(eventDataJsonObj.ToString(), CreateJsonOptions());

            var eventDataAssetType = JsonSerializer.Deserialize<AssetType>(data["assetType"].ToString(), CreateJsonOptions());
            eventDataAssetType.Should().BeEquivalentTo(expected["assetType"]);

            var eventDataAssetAddress = JsonSerializer.Deserialize<AssetAddress>(data["assetAddress"].ToString(), CreateJsonOptions());
            eventDataAssetAddress.Should().BeEquivalentTo(expected["assetAddress"]);

            var eventDataAssetCharacteristics = JsonSerializer.Deserialize<AssetCharacteristics>(data["assetCharacteristics"].ToString(), CreateJsonOptions());
            eventDataAssetCharacteristics.Should().BeEquivalentTo(expected["assetCharacteristics"]);

            var eventDataAssetManagement = JsonSerializer.Deserialize<AssetManagement>(data["assetManagement"].ToString(), CreateJsonOptions());
            eventDataAssetManagement.Should().BeEquivalentTo(expected["assetManagement"]);

            var eventDataAssetLocation = JsonSerializer.Deserialize<AssetLocation>(data["assetLocation"].ToString(), CreateJsonOptions());
            eventDataAssetLocation.Should().BeEquivalentTo(expected["assetLocation"]);

            var eventDataTenure = JsonSerializer.Deserialize<AssetTenureDb>(data["tenure"].ToString(), CreateJsonOptions());
            eventDataTenure.Should().BeEquivalentTo(expected["tenure"]);

        }
    }
}
