using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Kakadu.ActionApi.Controllers;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using Kakadu.DTO;
using System.Threading;

namespace Kakadu.ActionApi.Tests
{
    public class BaseControllerUnitTests
    {
        private readonly Mock<ILogger<RestController>> _loggerMock = new Mock<ILogger<RestController>>();
        private readonly Mock<IAnonymousServiceHttpClient> _anonymousServiceClientMock = new Mock<IAnonymousServiceHttpClient>();
        private readonly Mock<IAuthenticatedServiceHttpClient> _authenticatedServiceClientMock = new Mock<IAuthenticatedServiceHttpClient>();
        private readonly Mock<IDistributedCache> _cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void ClassShoudBeDerivedFromControllerBase()
        {
            typeof(BaseActionApiController)
                .Should()
                .BeDerivedFrom<ControllerBase>();
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsEmptyForNullHttpRequest()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetSoapActionHeader" && x.IsPrivate);

            var result = (string)method.Invoke(controller, new object[] { null });

            Assert.True(string.IsNullOrWhiteSpace(result.ToString()));
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsEmptyStringWhenNoSoapActionHeaderPresent()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" }
            });

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetSoapActionHeader" && x.IsPrivate);

            var result = (string)method.Invoke(controller, new object[] { headers });

            Assert.True(string.IsNullOrWhiteSpace(result.ToString()));
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsCorrectSoapActionForMalformedSoapActionHeader()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" },
                { "SOAPAction", "Calculate" }
            });

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetSoapActionHeader" && x.IsPrivate);

            var result = (string)method.Invoke(controller, new object[] { headers });
            
            Assert.Equal("Calculate", result);
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsSanitizedValueFromProperSoapActionHeader()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" },
                { "SOAPAction", "http://tempuri.org/Calculate" }
            });

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetSoapActionHeader" && x.IsPrivate);

            var result = (string)method.Invoke(controller, new object[] { headers });

            Assert.Equal("Calculate", result.ToString());
        }

        [Fact]
        public void GetRequestBodyAsync_Returns_Content_For_POST()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

             var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate);

            var httpRequestMethod = "POST";
            Stream body = new MemoryStream(Encoding.UTF8.GetBytes(@"
                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:tem='http://tempuri.org/'>
                    <soapenv:Header/>
                    <soapenv:Body>
                        <tem:Add>
                            <tem:intA>6</tem:intA>
                            <tem:intB>3</tem:intB>
                        </tem:Add>
                    </soapenv:Body>
                </soapenv:Envelope>
            "));

            var task = (Task<string>)method.Invoke(controller, new object[] { httpRequestMethod, body });
            
            var result = task.GetAwaiter().GetResult();

            Assert.NotNull(result);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetRequestBodyAsync_Returns_EmptyString_For_GET()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

             var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate);

            string httpRequestMethod = "GET";
            Stream body = new MemoryStream(Encoding.UTF8.GetBytes(@"
                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:tem='http://tempuri.org/'>
                    <soapenv:Header/>
                    <soapenv:Body>
                        <tem:Add>
                            <tem:intA>6</tem:intA>
                            <tem:intB>3</tem:intB>
                        </tem:Add>
                    </soapenv:Body>
                </soapenv:Envelope>
            "));

            var task = (Task<string>)method.Invoke(controller, new object[] { httpRequestMethod, body });
            
            var result = task.GetAwaiter().GetResult();
        
            Assert.NotNull(result);

            Assert.Empty(result);
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenStream_Is_Null()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate);

            Assert.ThrowsAsync<ArgumentNullException>(() => 
                (Task<string>)method.Invoke(controller, new object[] { "POST", null })
            );
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenMethod_Is_Null()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate);

            Assert.ThrowsAsync<ArgumentNullException>(() => 
                (Task<string>)method.Invoke(controller, new object[] { null, null })
            );
        }

        [Fact]
        public void CombinePaths_Throws_Exception_When_Param1_IsNull_Or_Whitespace()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "CombinePaths" && x.IsPrivate);

            Action act = () => method.Invoke(controller, new object[] { null, "abc" });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");


            act = () => method.Invoke(controller, new object[] { "", "abc" });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");

            act = () => method.Invoke(controller, new object[] { " ", "abc" });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");
        }

        [Fact]
        public void CombinePaths_Throws_Exception_When_Param2_IsNull_Or_Whitespace()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "CombinePaths" && x.IsPrivate);

            Action act = () => method.Invoke(controller, new object[] { "abc", null });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");


            act = () => method.Invoke(controller, new object[] { "abc", "" });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");

            act = () => method.Invoke(controller, new object[] { "abc", " " });
            act.Should().Throw<TargetInvocationException>()
                        .WithInnerException<Exception>()
                        .WithMessage("Can't combine paths as at least one part is null or empty");                
        }

        [Fact]
        public void CombinePaths_ReturnsCorrectPath()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "CombinePaths" && x.IsPrivate);

            var result = (string)method.Invoke(controller, new object[] { "post", "comments" });
            Assert.Equal("post/comments", result);
            
            result = (string)method.Invoke(controller, new object[] { "http://address/posts/", "/comments" });

            Assert.Equal("http://address/posts/comments", result);
        }

        [Fact]
        public void IntercepKnownRouteAsync_ThrowsExceptionForNullParameters()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO();

            var type = typeof(BaseActionApiController);

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, null, "/comments", service });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

            var response = new Mock<HttpResponse>();
            task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, null, service });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

            task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/comments", null });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);
        }

        [Fact]
        public async Task InterceptKnownRouteAsync_ReturnsTrueWhenNoKnownRouteFoundAndCantPassThrough()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = false
            };

            var type = typeof(BaseActionApiController);
            var response = new Mock<HttpResponse>();

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/path", service });
            var result = await task;

            result.Should().BeTrue();
        }

        [Fact]
        public async Task InterceptKnownRouteAsync_ReturnsFalseWhenNoKnownRouteFoundAndCanPassThrough()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true
            };

            var type = typeof(BaseActionApiController);
            var response = new Mock<HttpResponse>();

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/path", service });
            var result = await task;

            result.Should().BeFalse();
        }

        [Fact]
        public async Task InterceptKnownRouteAsync_ReturnsTrueWhenKnownRouteFoundAndCantPassThrough()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = false,
                KnownRoutes = new List<KnownRouteDTO> {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "GET",
                        RelativeUrl = "/path"
                    }
                }
            };

            var type = typeof(BaseActionApiController);
            var response = new Mock<HttpResponse>();

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/path", service });
            var result = await task;

            result.Should().BeTrue();
        }

        [Fact]
        public async Task InterceptKnownRouteAsync_ReturnsFalseWhenKnownRouteFoundAndCanPassThrough()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true,
                KnownRoutes = new List<KnownRouteDTO> {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "GET",
                        RelativeUrl = "/path"
                    }
                }
            };

            var type = typeof(BaseActionApiController);
            var response = new Mock<HttpResponse>();

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/path", service });
            var result = await task;

            result.Should().BeFalse();
        }

        [Fact]
        public async Task InterceptKnownRouteAsync_ReturnsTrueWhenKnownRouteWithAReplyFound()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var service = new ServiceDTO {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true,
                KnownRoutes = new List<KnownRouteDTO> {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "GET",
                        RelativeUrl = "/path",
                        Replies = new List<KnownRouteReplyDTO> {
                            new KnownRouteReplyDTO {
                                Id = Guid.NewGuid(),
                                StatusCode = 200,
                                ContentType = "application/json",
                                ContentEncoding = "utf-8",
                                ContentLength = 0
                            }
                        }
                    }
                }
            };

            var type = typeof(BaseActionApiController);
            var response = new Mock<HttpResponse>();

            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate);

            var task = (Task<bool>)method.Invoke(controller, new object[] { null, response.Object, "/path", service });
            var result = await task;

            result.Should().BeTrue();
        }

        [Fact]
        public void StoreReply_ThrowsExceptionsOnEmptyParameters()
        {
            var controller = new RestController(_loggerMock.Object, _anonymousServiceClientMock.Object, _authenticatedServiceClientMock.Object, _cacheMock.Object);
            var knownRoute = new KnownRouteDTO {
                Id = Guid.NewGuid(),
                MethodName = "GET",
                RelativeUrl = "/path",
                Replies = new List<KnownRouteReplyDTO> {
                    new KnownRouteReplyDTO {
                        Id = Guid.NewGuid(),
                        StatusCode = 200,
                        ContentType = "application/json",
                        ContentEncoding = "utf-8",
                        ContentLength = 0
                    }
                }
            };

            var type = typeof(BaseActionApiController);
            var method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "StoreReply" && x.IsPrivate);

            var cts = new CancellationTokenSource(1000);

            var task = (Task)method.Invoke(controller, new object[] { "", null, cts.Token });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

            task = (Task)method.Invoke(controller, new object[] { "dummy", null, cts.Token });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

            task = (Task)method.Invoke(controller, new object[] { "", knownRoute, cts.Token });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

            task = (Task)method.Invoke(controller, new object[] { null, knownRoute, cts.Token });
            Assert.ThrowsAsync<ArgumentNullException>(async () => await task);
        }
    }
}