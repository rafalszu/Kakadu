using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kakadu.ActionApi.Controllers;
using Kakadu.ActionApi.Interfaces;
using Kakadu.Common.Extensions;
using Kakadu.DTO;
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
        public void ClassShouldBeDecoratedAsApiController()
        {
            typeof(RestController).GetTypeInfo()
                .Should()
                .BeDecoratedWith<ApiControllerAttribute>()
                .And
                .BeDecoratedWith<ConsumesAttribute>();
        }

        [Fact]
        public void StartRecordingThrowsException_WhenNoAuthHeaderPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var tcs = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<HttpBadRequestException>(() => controller.StartRecordingAsync("dummy", tcs.Token));
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

            var result = await controller.StartRecordingAsync("dummy", tcs.Token);
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

            var result = await controller.StartRecordingAsync("dummy", tcs.Token);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void StopRecordingThrowsException_WhenNoAuthHeaderPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var tcs = new CancellationTokenSource(1000);
            Assert.ThrowsAsync<HttpBadRequestException>(() => controller.StopRecordingAsync("dummy", tcs.Token));
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

            var result = await controller.StopRecordingAsync("dummy", tcs.Token);
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

            var result = await controller.StopRecordingAsync("dummy", tcs.Token);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetStatus_ThrowsExceptionWhenServiceCodeIsEmpty(string serviceCode)
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);

            var tcs = new CancellationTokenSource(1000);

            Func<Task> func = () => controller.GetStatusAsync(serviceCode, tcs.Token);

            func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void GetStatus_ThrowsHttpBadRequestExceptionWhenNoAuthHeaderPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            var tcs = new CancellationTokenSource(1000);
            Func<Task> func = () => controller.GetStatusAsync("dummy", tcs.Token);

            func.Should()
                .ThrowAsync<HttpResponseException>();
        }

        [Fact]
        public async Task GetStatus_Returns200OK_ForValidToken()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            //byte[] boolBytes = true.ToByteArray();

            // cacheMock.Setup(x => 
            //     x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            // ).ReturnsAsync(boolBytes);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";

            var tcs = new CancellationTokenSource(1000);
            var result = await controller.GetStatusAsync("dummy", tcs.Token);

            result.Should()
                  .BeOfType<ActionResult<bool>>();
        }

        [Fact]
        public async Task GetStatus_Reutrns401_WhenNoValidTokenPresent()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
    
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.GetStatusAsync("dummy", tcs.Token);
            result
                .Should().BeOfType<ActionResult<bool>>()
                .Subject
                .Result.Should()
                       .BeOfType<UnauthorizedResult>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetStatus_Returns200OkWithExpectedValue(bool expected)
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            byte[] boolBytes = expected.ToByteArray();

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(boolBytes);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";

            var tcs = new CancellationTokenSource(1000);
            var result = await controller.GetStatusAsync("dummy", tcs.Token);

            result.Should()
                  .BeOfType<ActionResult<bool>>();

            result
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject
                .Value.As<bool>()
                .Should()
                .Be(expected);
        }

        [Fact]
        public async Task GetStatus_Returns200OkFalse_WhenRecordCacheKeyIsNotPresent()
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";

            var tcs = new CancellationTokenSource(1000);
            var result = await controller.GetStatusAsync("dummy", tcs.Token);

            result.Should()
                  .BeOfType<ActionResult<bool>>();

            result.As<ActionResult<bool>>()
                  .Value
                  .Should()
                  .BeFalse();
        }

        [Fact]
        public void TryValidateTokenAsync_ThrowsExceptionIfAuthHeaderNotPresent()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var tcs = new CancellationTokenSource(1000);
            Func<Task> func = () => controller.TryValidateTokenAsync(tcs.Token);

            func.Should()
                .ThrowAsync<HttpBadRequestException>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TryValidateTokenAsync_ShouldReturnValidityOfToken(bool expected)
        {
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
            
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.TryValidateTokenAsync(tcs.Token);
            result.Should()
                  .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetRecordingStatusAsync_ThrowsExceptionWhenServiceCodeIsNullOrEmpty(string serviceCode)
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            var tcs = new CancellationTokenSource(1000);

            Func<Task> func = () => controller.GetRecordingStatusAsync(serviceCode, tcs.Token);
            func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetRecordingStatusAsync_SetsIsRecordingFlagAccordingly(bool? expected)
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            var tcs = new CancellationTokenSource(1000);

            byte[] boolBytes = expected.ToByteArray();

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(boolBytes);

            var result = await controller.GetRecordingStatusAsync("dummy", tcs.Token);

            result.Should()
                  .NotBeNull()
                  .And
                  .Subject.As<ServiceCaptureStatusDTO>()
                  .IsRecording
                  .Should()
                  .Be(expected ?? false);
        }

        List<string> emptyList = new List<string>();

        [Fact]
        public async Task GetStatusAsync_ReturnsNullForEmptyList()
        {
            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            var tcs = new CancellationTokenSource(1000);

            var result = await controller.GetStatusesAsync(null, tcs.Token);
            result.Should()
                  .BeNull();

            result = await controller.GetStatusesAsync(new List<string>(), tcs.Token);
            result.Should()
                  .BeNull();
        }

        [Fact]
        public async Task GetStatusesAsync_Returns401_ForInvalidToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";
            

            var result = await controller.GetStatusesAsync(new List<string> { "dummy1" }, cts.Token);
            result.Result.Should()
                  .BeOfType<UnauthorizedResult>();
        }

        [Theory]
        [InlineData("dummy1", "dummy2", true, true)]
        [InlineData("dummy1", "dummy2", true, false)]
        [InlineData("dummy1", "dummy2", false, true)]
        [InlineData("dummy1", "dummy2", false, false)]
        public async Task GetStatusesAsync_ReturnsArrayOfServiceCaptureStatuses(string code1, string code2, bool expected1, bool expected2)
        {
            var cts = new CancellationTokenSource(1000);
            anonymousServiceHttpClientMock
                .Setup<Task<bool>>(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new RecordController(loggerMock.Object, anonymousServiceHttpClientMock.Object, cacheMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "authtoken";

            byte[] boolBytes1 = expected1.ToByteArray();
            byte[] boolBytes2 = expected2.ToByteArray();

            cacheMock.Setup(x => 
                x.GetAsync(code1, It.IsAny<CancellationToken>())
            ).ReturnsAsync(boolBytes1);
            
            cacheMock.Setup(x => 
                x.GetAsync(code2, It.IsAny<CancellationToken>())
            ).ReturnsAsync(boolBytes2);
           
            var result = await controller.GetStatusesAsync(new List<string> { code1, code2 }, cts.Token);

            result
                .Should()
                .NotBeNull();

            result
                .Value
                .Should()
                .NotBeNull()
                .And
                .HaveCount(2);
        }

    }
}