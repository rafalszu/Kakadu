using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Controllers;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class RecordControllerTests
    {
        Mock<ILogger<RecordController>> loggerMock = new Mock<ILogger<RecordController>>();
        Mock<IAnonymousServiceHttpClient> anonymousServiceHttpClientMock = new Mock<IAnonymousServiceHttpClient>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

        [Fact]
        public void StartRecordingThrowsException_WhenNoAuthHeaderPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var tcs = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<HttpBadRequestException>(() => controller.StartRecording("dummy", tcs.Token));
        }

        [Fact]
        public async Task StartRecordingReturns200OK_ForValidToken()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
    
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.StartRecording("dummy", tcs.Token);
            Assert.IsType<OkObjectResult>(result);
            
            OkObjectResult ok = (OkObjectResult)result;
            Assert.NotNull(ok.Value);

            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task StartRecordingReturns401_WhenNoValidTokenPresent()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
    
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.StartRecording("dummy", tcs.Token);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void StopRecordingThrowsException_WhenNoAuthHeaderPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var tcs = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<HttpBadRequestException>(() => controller.StopRecording("dummy", tcs.Token));
        }

        [Fact]
        public async Task StopRecordingReturns200OK_ForValidToken()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
    
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.StopRecording("dummy", tcs.Token);
            Assert.IsType<OkObjectResult>(result);
            
            OkObjectResult ok = (OkObjectResult)result;
            Assert.NotNull(ok.Value);

            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task StopRecordingReturns401_WhenNoValidTokenPresent()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
    
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.StopRecording("dummy", tcs.Token);
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}