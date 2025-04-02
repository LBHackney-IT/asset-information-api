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
            await WhenEditAssetApiIsCalled(id, requestObject, defaultIfMatch, false, false).ConfigureAwait(false);
        }

        public async Task WhenEditAssetAddressApiIsCalled(Guid id, object requestObject)
        {
            int? defaultIfMatch = 0;
            await WhenEditAssetApiIsCalled(id, requestObject, defaultIfMatch, true, false).ConfigureAwait(false);
        }

        public async Task WhenEditPropertyPatchApiIsCalled(Guid id, object requestObject)
        {
            int? defaultIfMatch = 0;
            await WhenEditAssetApiIsCalled(id, requestObject, defaultIfMatch, false, true).ConfigureAwait(false);
        }

        public async Task WhenEditAssetApiIsCalled(Guid id, object requestObject, int? ifMatch, bool addressEndpoint, bool patchEndpoint)
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";

            // setup request
            var uri = new Uri($"api/v1/assets/{id}{(addressEndpoint ? "/address" : "")}{(patchEndpoint ? "/patch" : "")}", UriKind.Relative);
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

        public async Task ThenUnauthorizedIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
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
            databaseResponse.PatchId.Should().Be(assetFixture.ExistingAsset.PatchId);
            databaseResponse.AreaId.Should().Be(assetFixture.ExistingAsset.AreaId);
        }

        public async Task ThenEditAssetBadRequestIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseEntity = JsonSerializer.Deserialize<ErrorResponse>(responseContent, CreateJsonOptions());

            responseEntity.Should().BeOfType(typeof(ErrorResponse));

            responseEntity.Error.Should().Be(StatusCodes.Status400BadRequest);
        }

        public async Task TheAssetHasBeenUpdatedInTheDatabase(AssetsFixture assetFixture, bool editPropertyPatch)
        {
            var databaseResponse = await _dbContext.LoadAsync<AssetDb>(assetFixture.AssetId).ConfigureAwait(false);

            databaseResponse.Id.Should().Be(assetFixture.ExistingAsset.Id);
            databaseResponse.AssetCharacteristics.ToString().Should().Be(assetFixture.EditAsset.AssetCharacteristics.ToDatabase().ToString());
            databaseResponse.AssetManagement.ToString().Should().Be(assetFixture.EditAsset.AssetManagement.ToString());
            databaseResponse.ParentAssetIds.Should().Be(assetFixture.EditAsset.ParentAssetIds);
            databaseResponse.RootAsset.Should().Be(assetFixture.EditAsset.RootAsset);
            databaseResponse.AssetLocation.ToString().Should().Be(assetFixture.EditAsset.AssetLocation.ToString());
            if (editPropertyPatch)
            {
                databaseResponse.PatchId.Should().Be(assetFixture.EditPropertyPatch.PatchId);
                databaseResponse.AreaId.Should().Be(assetFixture.EditPropertyPatch.AreaId);
            }
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
                    { "rootAsset", assetFixture.Asset.RootAsset },
                    { "parentAssetIds", assetFixture.Asset.ParentAssetIds },
                    { "assetCharacteristics", assetFixture.Asset.AssetCharacteristics },
                    { "assetManagement", assetFixture.Asset.AssetManagement },
                    { "assetLocation", assetFixture.Asset.AssetLocation },
                };
                var expectedNewData = new Dictionary<string, object>
                {
                    { "rootAsset", dbRecord.RootAsset },
                    { "parentAssetIds", dbRecord.ParentAssetIds },
                    { "assetCharacteristics", dbRecord.AssetCharacteristics },
                    { "assetManagement", dbRecord.AssetManagement },
                    { "assetLocation", dbRecord.AssetLocation },
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

        public async Task ThenTheAssetAddressOrPropertyPatchUpdatedEventIsRaised(AssetsFixture assetFixture, ISnsFixture snsFixture)
        {
            var dbRecord = await _dbContext.LoadAsync<AssetDb>(assetFixture.Asset.Id).ConfigureAwait(false);

            Action<EntityEventSns> verifyFunc = (actual) => { };

            var snsVerifer = snsFixture.GetSnsEventVerifier<EntityEventSns>();
            var snsResult = await snsVerifer.VerifySnsEventRaised(verifyFunc).ConfigureAwait(false);
            if (snsVerifer.LastException != null)
                //throw new Exception("Assert test: " + snsFixture.AmazonSQS.Config.ServiceURL + " and " + snsFixture.SimpleNotificationService.Config.RegionEndpointServiceName + " and " + assetFixture.Asset.Id);
                throw snsVerifer.LastException;
        }

        private void VerifyEventData(object eventDataJsonObj, Dictionary<string, object> expected)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(eventDataJsonObj.ToString(), CreateJsonOptions());

            var eventDataAssetCharacteristics = JsonSerializer.Deserialize<AssetCharacteristics>(data["assetCharacteristics"].ToString(), CreateJsonOptions());
            eventDataAssetCharacteristics.Should().BeEquivalentTo(expected["assetCharacteristics"]);

            var eventDataAssetManagement = JsonSerializer.Deserialize<AssetManagement>(data["assetManagement"].ToString(), CreateJsonOptions());
            eventDataAssetManagement.Should().BeEquivalentTo(expected["assetManagement"]);

            var eventDataAssetLocation = JsonSerializer.Deserialize<AssetLocation>(data["assetLocation"].ToString(), CreateJsonOptions());
            eventDataAssetLocation.Should().BeEquivalentTo(expected["assetLocation"]);
        }
    }
}
