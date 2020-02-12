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

namespace Kakadu.ActionApi.Tests
{
    public class SoapControllerTests
    {
        Mock<ILogger<SoapController>> loggerMock = new Mock<ILogger<SoapController>>();
        Mock<IAnonymousServiceHttpClient> anonymousServiceClientMock = new Mock<IAnonymousServiceHttpClient>();
        Mock<IAuthenticatedServiceHttpClient> authenticatedServiceClientMock = new Mock<IAuthenticatedServiceHttpClient>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void ClassShouldBeDecoratedAsApiController()
        {
            typeof(SoapController).GetTypeInfo()
                .Should()
                .BeDecoratedWith<ApiControllerAttribute>()
                .And
                .BeDecoratedWith<ConsumesAttribute>()
                .And
                .BeDerivedFrom<BaseActionApiController>();
        }

        [Fact]
        public void ProcessRequestShouldRespondTo_POST_ONLY()
        {
            typeof(SoapController).GetTypeInfo()
                .GetMethod("ProcessRequest")
                .Should()
                .BeDecoratedWith<HttpPostAttribute>()
                .And
                .NotBeDecoratedWith<HttpGetAttribute>()
                .And
                .NotBeDecoratedWith<HttpPutAttribute>()
                .And
                .NotBeDecoratedWith<HttpPatchAttribute>()
                .And
                .NotBeDecoratedWith<HttpDeleteAttribute>();
        }

        [Fact]
        public void ProcessReuqest_ThrowsExceptionWhenEmptyServiceCodeProvided()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            CancellationTokenSource cts = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<ArgumentNullException>(async () => await controller.ProcessRequest(null, cts.Token));
        }

        [Fact]        
        public void GetKnownRoute_ThrowsExceptionWhenEmptyServicePassed()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            Assert.Throws<ArgumentNullException>(() => controller.GetKnownRoute(null, "path", null));
        }

        [Fact]
        public void GetKnownRoute_ThrowsExceptionWhenEmptyRelativePathPassed()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var service = new ServiceDTO();

            Assert.Throws<ArgumentNullException>(() => controller.GetKnownRoute(service, "", null));
        }

        [Fact]
        public void GetKnownRoute_ReturnsNullWhenKnownRoutesIsNull()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var service = new ServiceDTO
            {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true
            };

            var result = controller.GetKnownRoute(service, "/Calculate.asmx", "Add");
            Assert.Null(result);
        }

        [Fact]
        public void GetKnownRoute_ReturnsNullWhenActionIsNull()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var service = new ServiceDTO
            {
                Code = "dummy",
                Name = "dummy",
                Id = Guid.NewGuid(),
                Address = new Uri("http://address.to"),
                UnkownRoutesPassthrough = true,
                KnownRoutes = new List<KnownRouteDTO> {
                    new KnownRouteDTO {
                        Id = Guid.NewGuid(),
                        MethodName = "POST",
                        RelativeUrl = "/Calculate.asmx",
                        Action = "Add"
                    }
                }
            };

            var result = controller.GetKnownRoute(service, "/Calculate.asmx", null);
            Assert.Null(result);
        }

        [Fact]
        public void GetKnownRoute_ReturnsKnownRouteWhenRelativePathMatches()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

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
                        MethodName = "POST",
                        RelativeUrl = "/Calculate.asmx",
                        Action = "Add"
                    }
                }
            };

            var result = controller.GetKnownRoute(service, "/Calculate.asmx", "Add");
            Assert.NotNull(result);

            Assert.Equal<Guid>(service.KnownRoutes[0].Id, result.Id);
        }

        [Fact]
        public void GetKnownRoute_ReturnsKnownRouteWhenRelativePathMatchesCaseMismatch()
        {
            var controller = new SoapController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

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
                        MethodName = "POST",
                        RelativeUrl = "/Calculate.ASMX",
                        Action = "Add"
                    }
                }
            };

            var result = controller.GetKnownRoute(service, "/Calculate.asmx", "Add");
            Assert.NotNull(result);

            Assert.Equal<Guid>(service.KnownRoutes[0].Id, result.Id);
        }
    }
}