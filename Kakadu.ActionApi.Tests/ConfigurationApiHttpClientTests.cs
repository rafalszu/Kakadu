using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kakadu.ActionApi.HttpClients;
using Kakadu.ActionApi.Interfaces;
using Kakadu.Common.Extensions;
using Kakadu.Common.HttpClients;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class ConfigurationApiHttpClientTests
    {
        Mock<ILogger<ConfigurationApiHttpClient>> loggerMock = new Mock<ILogger<ConfigurationApiHttpClient>>();

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        public ConfigurationApiHttpClientTests()
        {
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                });
        }

        Fact]
        public void ClassShouldBeDerivedFromHttpClientBase()
        {
            typeof(ConfigurationApiHttpClient)
                .Should()
                .BeDerivedFrom<HttpClientBase>()
                .And
                .Implement<IConfigurationApiHttpClient>();
        }

        [Fact]
        public void Register_ThrowsExceptionForNullOrEmptyUrl()
        {
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var client = new ConfigurationApiHttpClient(httpClient, loggerMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            Func<Task> func = () => client.Register(null, cts.Token);
            func.Should().ThrowAsync<ArgumentNullException>();

            func = () => client.Register("", cts.Token);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void Unregister_ThrowsExceptionForNullOrEmptyUrl()
        {
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var client = new ConfigurationApiHttpClient(httpClient, loggerMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            Func<Task> func = () => client.Unregister(null, cts.Token);
            func.Should().ThrowAsync<ArgumentNullException>();

            func = () => client.Unregister("", cts.Token);
            func.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}