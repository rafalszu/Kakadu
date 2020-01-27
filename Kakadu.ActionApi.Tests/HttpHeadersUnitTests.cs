using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Kakadu.ActionApi.Extensions;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ConvertsToDictionary()
        {
            HttpRequestMessage msg = new HttpRequestMessage();
            msg.Headers.Add("Test-Header", "HeaderValue");
            msg.Headers.Add("Authorization", "Bearer mysecrettoken");

            var dict = msg.Headers.AsDictionary();

            Assert.NotNull(dict);

            Assert.NotEmpty(dict);

            Assert.Equal(2, dict.Count);

            Assert.Contains("Test-Header", dict.Keys);
        }
    }
}
