using System;
using System.Net;
using System.Net.Http;
using Kakadu.ActionApi.Extensions;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class KnownRouteModelUnitTests
    {
        [Fact]
        public void HttpResponseMessage_Converts_To_KnownRouteModel()
        {
            HttpResponseMessage msg = new HttpResponseMessage
            {
                RequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://some.address/posts/1#comments")
                },
                StatusCode =  HttpStatusCode.OK,
                Headers = {{ "Test-Header", "HeaderValue" }},
                Content = new StringContent("{\"test\": \"value\"}", System.Text.Encoding.UTF8, "application/json")
            };
            
            var knownRoute = msg.ToKnownRouteDTO();

            Assert.NotNull(knownRoute);
        
            Assert.Equal("GET", knownRoute.MethodName);

            Assert.Empty(knownRoute.Action);

            Assert.Equal("/posts/1#comments", knownRoute.RelativeUrl);

            Assert.NotNull(knownRoute.Replies);

            Assert.True(1 == knownRoute.Replies.Count);

            var reply = knownRoute.Replies[0];

            Assert.Equal(200, reply.StatusCode);

            Assert.Equal("application/json", reply.ContentType);

            Assert.Equal("utf-8", reply.ContentTypeCharset);

            Assert.Equal("application/json; charset=utf-8", reply.ContentTypeString);

            Assert.Equal("eyJ0ZXN0IjogInZhbHVlIn0=", reply.ContentBase64); // eyJ0ZXN0IjogInZhbHVlIn0= = {"test": "value"}

            Assert.NotNull(reply.Headers);

            Assert.NotEmpty(reply.Headers);

            Assert.True(1 == reply.Headers.Count);

            Assert.Contains("Test-Header", reply.Headers.Keys);
        }
    }
}