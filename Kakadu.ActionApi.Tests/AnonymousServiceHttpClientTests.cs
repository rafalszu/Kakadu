using System;
using System.Net;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kakadu.DTO;
using System.Threading;
using Kakadu.ActionApi.HttpClients;
using System.Net.Http;
using Moq.Protected;
using Kakadu.Common.HttpClients;
using Kakadu.Common.Extensions;
using Newtonsoft.Json;
using System.Linq;

namespace Kakadu.ActionApi.Tests
{
    public class AnonymousServiceHttpClientTests
    {
        Mock<ILogger<AnonymousServiceHttpClient>> loggerMock = new Mock<ILogger<AnonymousServiceHttpClient>>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void ClassShouldBeDerivedFromHttpClientBase()
        {
            typeof(AnonymousServiceHttpClient)
                .Should()
                .BeDerivedFrom<HttpClientBase>()
                .And
                .Implement<IAnonymousServiceHttpClient>();
        }

        [Fact]
        public void GetByCodeAsync_ThrowsExceptionWhenServiceCodeIsNulOrEmpty()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{'id':1,'value':'1'}]"),
                });
            
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            AnonymousServiceHttpClient client = new AnonymousServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            
            Func<Task> func = async () => await client.GetByCodeAsync(null, cts.Token);
                  
            func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByCodeAsync_ReturnDtoWhenServiceCodeIsPresentInCache()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{'id':1,'value':'1'}]"), // return random array
                })
                .Verifiable();

            var service = new ServiceDTO
            {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true,
                KnownRoutes = new List<KnownRouteDTO>
                {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "GET",
                        RelativeUrl = "/path"
                    }
                }
            };

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(service.ToByteArray());

                
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.address/"),
            };

            AnonymousServiceHttpClient client = new AnonymousServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            
            var result = await client.GetByCodeAsync("dummy", cts.Token);
            
            result
                .Should()
                .NotBeNull()
                .And
                .BeOfType<ServiceDTO>()
                .And
                .BeEquivalentTo<ServiceDTO>(service);
        }

        [Fact]
        public async Task GetByCodeAsync_ReturnDtoWhenServiceCodeIsNotPresentInCache()
        {
            var service = new ServiceDTO
            {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true,
                KnownRoutes = new List<KnownRouteDTO>
                {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "GET",
                        RelativeUrl = "/path"
                    }
                }
            };
            var json = JsonConvert.SerializeObject(service);

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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"), // actual service object
                })
                .Verifiable();

            byte[] nullByteArray = null;

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(nullByteArray);
                
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.address/"),
            };

            AnonymousServiceHttpClient client = new AnonymousServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            
            var result = await client.GetByCodeAsync("dummy", cts.Token);
            
            result
                .Should()
                .NotBeNull()
                .And
                .BeOfType<ServiceDTO>()
                .And
                .BeEquivalentTo<ServiceDTO>(service);
        }

        [Fact]
        public async Task ValidateTokenAsync_ReturnsFalseWhenAccessTokenIsNullOrEmpty()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(true.ToString()),
                });
            
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.address/"),
            };

            AnonymousServiceHttpClient client = new AnonymousServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            var result = await client.ValidateTokenAsync(null, cts.Token);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_AddsAuthorizationHeader()
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
                BaseAddress = new Uri("http://api.address/"),
            };

            AnonymousServiceHttpClient client = new AnonymousServiceHttpClient(httpClient, loggerMock.Object, cacheMock.Object);
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            await client.ValidateTokenAsync("Bearer secretAccessToken", cts.Token);

            client.CustomRequestHeaders.Should()
                                        .ContainKey("Authorization")
                                        .And
                                        .Match(kv => kv["Authorization"].ElementAt(0) == "Bearer secretAccessToken");
        }
    }
}