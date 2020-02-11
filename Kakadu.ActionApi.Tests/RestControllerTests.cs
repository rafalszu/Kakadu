using FluentAssertions;
using Kakadu.ActionApi.Controllers;
using Kakadu.ActionApi.Interfaces;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Kakadu.ActionApi.Tests
{
    public class RestControllerTests
    {
        Mock<ILogger<RestController>> loggerMock = new Mock<ILogger<RestController>>();
        Mock<IAnonymousServiceHttpClient> anonymousServiceClientMock = new Mock<IAnonymousServiceHttpClient>();
        Mock<IAuthenticatedServiceHttpClient> authenticatedServiceClientMock = new Mock<IAuthenticatedServiceHttpClient>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void ClassShouldBeDecoratedAsApiController()
        {
            typeof(RestController).GetTypeInfo()
                .Should()
                .BeDecoratedWith<ApiControllerAttribute>()
                .And
                .BeDecoratedWith<ConsumesAttribute>()
                .And
                .BeDerivedFrom<BaseActionApiController>();
        }

        [Fact]
        public void ProcessRequestShouldRespondTo_GET_POST_PUT_PATCH_DELETE()
        {
            typeof(RestController).GetTypeInfo()
                .GetMethod("ProcessRequest")
                .Should()
                .BeDecoratedWith<HttpGetAttribute>()
                .And
                .BeDecoratedWith<HttpPostAttribute>()
                .And
                .BeDecoratedWith<HttpPutAttribute>()
                .And
                .BeDecoratedWith<HttpPatchAttribute>()
                .And
                .BeDecoratedWith<HttpDeleteAttribute>();
        }

        [Fact]
        public void ProcessReuqest_ThrowsExceptionWhenEmptyServiceCodeProvided()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            CancellationTokenSource cts = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<ArgumentNullException>(async () => await controller.ProcessRequest(null, cts.Token));
        }

        [Fact]        
        public void GetKnownRoute_ThrowsExceptionWhenEmptyServicePassed()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            Assert.Throws<ArgumentNullException>(() => controller.GetKnownRoute(null, "path", null));
        }

        [Fact]
        public void GetKnownRoute_ThrowsExceptionWhenEmptyRelativePathPassed()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var service = new ServiceDTO();

            Assert.Throws<ArgumentNullException>(() => controller.GetKnownRoute(service, "", null));
        }

        [Fact]
        public void GetKnownRoute_ReturnsNullIfKnownRoutesIsNull()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var service = new ServiceDTO
            {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true
            };

            var result = controller.GetKnownRoute(service, "path", null);
            Assert.Null(result);
        }

        [Fact]
        public void GetKnownRoute_ReturnsKnownRouteWhenRelativePathMatches()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

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

            var result = controller.GetKnownRoute(service, "/path", null);
            Assert.NotNull(result);

            Assert.Equal<Guid>(service.KnownRoutes[0].Id, result.Id);
        }

        [Fact]
        public void GetKnownRoute_ReturnsKnownRouteWhenRelativePathMatchesCaseMismatch()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

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

            var result = controller.GetKnownRoute(service, "/Path", null);
            Assert.NotNull(result);

            Assert.Equal<Guid>(service.KnownRoutes[0].Id, result.Id);
        }

        
    }
}