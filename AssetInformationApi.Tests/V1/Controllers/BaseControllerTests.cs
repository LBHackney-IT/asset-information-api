using AssetInformationApi.V1.Controllers;
using FluentAssertions;
using Hackney.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Xunit;

namespace AssetInformationApi.Tests.V1.Controllers
{
    public class BaseControllerTests
    {
        private BaseController _sut;
        private ControllerContext _controllerContext;
        private HttpContext _stubHttpContext;

        public BaseControllerTests()
        { }

        private void ConstructController()
        {
            _stubHttpContext = new DefaultHttpContext();
            _controllerContext = new ControllerContext(new ActionContext(_stubHttpContext, new RouteData(), new ControllerActionDescriptor()));
            _sut = new BaseController();

            _sut.ControllerContext = _controllerContext;
        }

        [Fact]
        public void GetCorrelationShouldThrowExceptionIfCorrelationHeaderUnavailable()
        {
            ConstructController();

            // Arrange + Act + Assert
            _sut.Invoking(x => x.GetCorrelationId())
                .Should().Throw<KeyNotFoundException>()
                .WithMessage("Request is missing a correlationId");
        }

        [Fact]
        public void GetCorrelationShouldReturnCorrelationIdWhenExists()
        {
            ConstructController();

            // Arrange
            _stubHttpContext.Request.Headers.Append(HeaderConstants.CorrelationId, "123");

            // Act
            var result = _sut.GetCorrelationId();

            // Assert
            result.Should().BeEquivalentTo("123");
        }

        [Fact]
        public void ConfigureJsonSerializerTest()
        {
            BaseController.ConfigureJsonSerializer();

            JsonConvert.DefaultSettings.Should().NotBeNull();
            var settings = JsonConvert.DefaultSettings();
            settings.Formatting.Should().Be(Formatting.Indented);
            settings.ContractResolver.GetType().Should().Be(typeof(CamelCasePropertyNamesContractResolver));
            settings.DateTimeZoneHandling.Should().Be(DateTimeZoneHandling.Utc);
            settings.DateFormatHandling.Should().Be(DateFormatHandling.IsoDateFormat);
        }
    }
}
