using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using Kakadu.DTO;
using System.Collections.Generic;
using Kakadu.Common.HttpClients;
using System.Net.Http;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using Kakadu.DTO.HttpExceptions;

namespace Kakadu.Common.Tests
{
    public class HttpClientBaseTests
    {
        Mock<ILogger<HttpClientBase>> loggerMock = new Mock<ILogger<HttpClientBase>>();

        [Fact]
        public async Task StreamToStringAsync_ReturnsNullWhenStreamIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = await httpClientBase.StreamToStringAsync(null);
            result.Should().BeNull();
        }

        [Fact]
        public async Task StreamToStringAsync_ReturnsValueWhenStreamHasValue()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("some value")))
            {
                var result = await httpClientBase.StreamToStringAsync(ms);

                result.Should().BeEquivalentTo("some value");
            }
        }

        [Fact]
        public void DesrializeJsonFromStream_ReturnsNullWhenStreamIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = httpClientBase.DeserializeJsonFromStream<string>(null);
            result.Should().BeNull();
        }

        [Fact]
        public void DeserializeJsonFromStream_ReturnsValueWhenStreamHasValue()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("true")))
            {
                var result = httpClientBase.DeserializeJsonFromStream<bool>(ms);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void TryDeserialize_ReturnsNullWhenReaderIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = httpClientBase.TryDeserialize<ServiceDTO>(null, true);
            
            result.Should().BeNull();
        }

        [Fact]
        public void TryDeserialize_ReturnsDeserializedService()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            ServiceDTO dto = new ServiceDTO { Code = "dummyservice" };

            using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"Code\":\"dummyservice\"}")))
            {
                using(StreamReader streamReader = new StreamReader(ms))
                {
                    using(JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        var result = httpClientBase.TryDeserialize<ServiceDTO>(reader, true);

                        result.Should().BeEquivalentTo(dto);
                    }
                }
            }
        }

        [Fact]
        public void TryDeserialize_ReturnsNullWhenResponseNotUnderstandableAndThrowIsFalse()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
            {
                using(StreamReader streamReader = new StreamReader(ms))
                {
                    using(JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        var result = httpClientBase.TryDeserialize<ServiceDTO>(reader, false);

                        result.Should().BeNull();
                    }
                }
            }
        }

        [Fact]
        public void TryDeserialize_ThrowsExceptionWhenResponseNotUnderstandableAndThrowIsTrue()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
            {
                using(StreamReader streamReader = new StreamReader(ms))
                {
                    using(JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        Action act = () => httpClientBase.TryDeserialize<bool>(reader, true); // use non-nullable here

                        act.Should()
                           .Throw<Exception>()
                           .WithMessage("Response not understandable");
                    }
                }
            }
        }

        [Fact]
        public void CreateHttpContent_ReturnsNullWhenContentIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = httpClientBase.CreateHttpContent(null);

            result.Should().BeNull();
        }

        [Fact]
        public void CreateHttpContent_ReturnsValueWhenContentHasValue()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = httpClientBase.CreateHttpContent("value");

            result.Should().NotBeNull();
        }

        [Fact]
        public void SerializeJsonIntoStream_LeavesStreamIntactWhenContentIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream())
            {
                httpClientBase.SerializeJsonIntoStream(null, ms);

                ms.Should().NotBeNull();

                ms.Length.Should().Be(0);
            }
        }

        [Fact]
        public void SerializeJsonIntoStream_FillsInStreamWithData()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            using(MemoryStream ms = new MemoryStream())
            {
                httpClientBase.SerializeJsonIntoStream("value", ms); // stream actually contains "value"

                ms.Should().NotBeNull();

                var msValue = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                msValue.Should().Be("\"value\"");
            }
        }

        [Fact]
        public void AddCustomRequestHeaders_AddsHeaders()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpClientBase.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { "x-custom-header", new List<string> { "value" } }
            };

            httpClientBase.AddCustomRequestHeaders(httpRequest);

            httpRequest.Headers
                       .Should()
                       .NotBeEmpty();
            
            httpRequest.Headers
                        .First()
                        .As<KeyValuePair<string, IEnumerable<string>>>()
                        .Key
                        .Should().Be("x-custom-header");
        }

        [Fact]
        public void AddCustomRequestHeaders_DoesNotAddHeadersWithoutValues()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpClientBase.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { "x-custom-header", new List<string>() }
            };

            httpClientBase.AddCustomRequestHeaders(httpRequest);

            httpRequest.Headers.Should().BeEmpty();

            httpRequest = new HttpRequestMessage();
            httpClientBase.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { "x-custom-header", null }
            };

            httpClientBase.AddCustomRequestHeaders(httpRequest);

            httpRequest.Headers.Should().BeEmpty();
        }

        [Fact]
        public async Task TryGetContentAsync_ReturnsNullWhenResponseIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);

            var result = await httpClientBase.TryGetContentAsync<string>(null);

            result.Should().BeNull();
        }

        [Fact]
        public async Task TryGetContentAsync_ReturnsNullWhenResponseContentIsNull()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);
            HttpResponseMessage response = new HttpResponseMessage();

            var result = await httpClientBase.TryGetContentAsync<string>(response);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadRequest)]
        public void TryGetContentAsync_ThrowsExceptionWhenStatusCodeDoesNotIndicateSuccess(HttpStatusCode statusCode)
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);
            HttpResponseMessage response = new HttpResponseMessage {
                StatusCode = statusCode,
                Content = new StringContent("error!")
            };

            Func<Task> func = () => httpClientBase.TryGetContentAsync<string>(response);
            func.Should()
                .ThrowAsync<HttpResponseException>();
        }

        // [Theory]
        // [InlineData(Boolean.GetType(), "true")]
        // [InlineData(typeof(string), "value")]
        // [InlineData(typeof(int), "234")]
        // [InlineData(typeof(ServiceDTO), "{\"Code\":\"dummyservice\"}")]
        // public async Task TryGetContentAsync_ReturnsDesrializedContent(Type expectedType, string content)
        // {
        //     HttpClient httpClient = new HttpClient();
        //     HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);
        //     HttpResponseMessage response = new HttpResponseMessage {
        //         StatusCode = HttpStatusCode.OK,
        //         Content = new StringContent(content)
        //     };

        //     MethodInfo method = typeof(HttpClientBase).GetMethod("TryGetContentAsync");
        //     MethodInfo generic = method.MakeGenericMethod(expectedType);
        //     var result = await (Task<object>)generic.Invoke(this, new object[] { response });

        //     result.Should()
        //           .NotBeNull()
        //           .And
        //           .BeAssignableTo(expectedType);
        // }
        [Fact]
        public async Task TryGetContentAsync_ReturnsDeserializedContent_ForBoolean()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);
            HttpResponseMessage response = new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("true")
            };

            var result = await httpClientBase.TryGetContentAsync<bool>(response);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task TryGetContentAsync_ReturnsDeserializedContent_ForDTO()
        {
            HttpClient httpClient = new HttpClient();
            HttpClientBase httpClientBase = new HttpClientBase(httpClient, loggerMock.Object);
            HttpResponseMessage response = new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"Code\":\"dummyservice\"}")
            };

            var result = await httpClientBase.TryGetContentAsync<ServiceDTO>(response);

            result.Should().NotBeNull();

            result.Code.Should().Be("dummyservice");
        }
    }
}