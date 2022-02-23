using AssetInformationApi.V1.Domain;
using FluentAssertions;
using System;
using Xunit;

namespace AssetInformationApi.Tests.V1.Domain
{
    public class AssetTenureTests
    {
        private readonly AssetTenure _classUnderTest;

        public AssetTenureTests()
        {
            _classUnderTest = new AssetTenure();
        }

        [Fact]
        public void GivenAnAssetTenureWhenEndDateIsNullThenIsActiveShouldBeTrue()
        {
            // given + when + then
            _classUnderTest.IsActive.Should().BeTrue();
        }

        [Fact]
        public void GivenAnAssetTenureWhenEndDateIsGreaterThanCurrentDateThenIsActiveShouldBeTrue()
        {
            // given
            _classUnderTest.EndOfTenureDate = DateTime.Now.AddDays(1);

            // when + then
            _classUnderTest.IsActive.Should().BeTrue();
        }

        [Fact]
        public void GivenAnAssetTenureWhenEndDateIsMinimumDateThanCurrentDateThenIsActiveShouldBeTrue()
        {
            // given
            _classUnderTest.EndOfTenureDate = DateTime.Parse("1900-01-01");

            // when + then
            _classUnderTest.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GivenAnAssetTenureWhenEndDateIsLessThanCurrentDateThenIsActiveShouldBeFalse(int daysToAdd)
        {
            // given
            _classUnderTest.EndOfTenureDate = DateTime.UtcNow.AddDays(daysToAdd);

            // when + then
            _classUnderTest.IsActive.Should().BeFalse();
        }
    }
}
