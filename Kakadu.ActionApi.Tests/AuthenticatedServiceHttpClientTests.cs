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
    public class AuthenticatedServiceHttpClientTests
    {
        Mock<ILogger<AuthenticatedServiceHttpClient>> loggerMock = new Mock<ILogger<AuthenticatedServiceHttpClient>>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void ClassShouldBeDerivedFromHttpClientBase()
        {
            typeof(AuthenticatedServiceHttpClient)
                .Should()
                .BeDerivedFrom<HttpClientBase>()
                .And
                .Implement<IAuthenticatedServiceHttpClient>();
        }

        [Fact]
        public void AuthenticatedServiceHttpClient_ThrowsExceptionWhenNoAccessTokenPresentInCache()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
            
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            byte[] emptyByteArray = new byte[0];
            cacheMock.Setup(x => 
                x.Get(KakaduConstants.ACCESS_TOKEN)
            ).Returns(emptyByteArray);

            Func<AuthenticatedServiceHttpClient> func = () => new AuthenticatedServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            func.Should().Throw<HttpUnauthorizedException>();

            byte[] nullByteArray = null;
            cacheMock.Setup(x => 
                x.Get(KakaduConstants.ACCESS_TOKEN)
            ).Returns(nullByteArray);

            func = () => new AuthenticatedServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            func.Should().Throw<HttpUnauthorizedException>();
        }

        [Fact]
        public void AuthenticatedServiceHttpClient_ThrowsExceptionWhithMalformedAccessToken()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
            
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes("secretAccessToken");

            cacheMock.Setup(x => 
                x.Get(KakaduConstants.ACCESS_TOKEN)
            ).Returns(tokenBytes);

            Func<AuthenticatedServiceHttpClient> func = () => new AuthenticatedServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            func.Should().Throw<Exception>().WithMessage("Wrong bearer token format");
        }

        [Fact]
        public void AuthenticatedServiceHttpClient_DefaultRequestHeaderContainsAuthorization()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
            
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes("Bearer secretAccessToken");

            cacheMock.Setup(x => 
                x.Get(KakaduConstants.ACCESS_TOKEN)
            ).Returns(tokenBytes);

            var client = new AuthenticatedServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);

            httpClient.DefaultRequestHeaders
                      .Should()
                      .NotBeEmpty()
                      .And
                      .ContainSingle(header => header.Key == "Authorization" && header.Value.Contains("Bearer secretAccessToken"));
        }
    }
}