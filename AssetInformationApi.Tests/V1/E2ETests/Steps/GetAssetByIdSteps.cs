using FluentAssertions;
using Hackney.Core.Testing.Shared.E2E;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Boundary.Response;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;

namespace AssetInformationApi.Tests.V1.E2ETests.Steps
{
    public class GetAssetByIdSteps : BaseSteps
    {
        public GetAssetByIdSteps(HttpClient httpClient) : base(httpClient)
        { }

        public async Task WhenTheGetApiIsCalled(string id)
        {
            var route = $"api/v1/assets/{id}";
            var uri = new Uri(route, UriKind.Relative);
            _lastResponse = await _httpClient.GetAsync(uri).ConfigureAwait(false);
        }

        private async Task<AssetResponseObject> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<AssetResponseObject>(responseContent, _jsonOptions);
            return apiResult;
        }

        public async Task ThenTheAssetDetailsAreReturned(AssetDb expectedAssetDb)
        {
            var apiAsset = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            var expected = expectedAssetDb.ToDomain().ToResponse();
            apiAsset.Should().BeEquivalentTo(expected);
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
