using FluentAssertions;
using Kakadu.ActionApi.Extensions;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Sanitize_ReturnsSameStringWhenStringIsNullOrEmpty(string s)
        {
            s.Sanitize()
             .Should()
             .BeSameAs(s);
        }

        [Theory]
        [InlineData("Add1")]
        [InlineData("Add1\\")]
        [InlineData("/Add1")]
        [InlineData("/Add\\1")]
        [InlineData("@Add1")]
        [InlineData("Add&1")]
        public void Sanitize_ReturnsOnlyLettersAndDigits(string s)
        {
            s.Sanitize()
             .Should()
             .BeEquivalentTo("Add1");
        }
    }
}