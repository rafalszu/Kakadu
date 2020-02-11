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

namespace Kakadu.ActionApi.Tests
{
    public class BaseControllerUnitTests
    {
        Mock<ILogger<RestController>> loggerMock = new Mock<ILogger<RestController>>();
        Mock<IAnonymousServiceHttpClient> anonymousServiceClientMock = new Mock<IAnonymousServiceHttpClient>();
        Mock<IAuthenticatedServiceHttpClient> authenticatedServiceClientMock = new Mock<IAuthenticatedServiceHttpClient>();
        Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();

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
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetSoapActionHeader" && x.IsPrivate)
                .First();

            var result = (string)method.Invoke(controller, new object[] { null });

            Assert.True(string.IsNullOrWhiteSpace(result.ToString()));
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsEmptyStringWhenNoSoapActionHeaderPresent()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" }
            });

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetSoapActionHeader" && x.IsPrivate)
                .First();

            var result = (string)method.Invoke(controller, new object[] { headers });

            Assert.True(string.IsNullOrWhiteSpace(result.ToString()));
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsCorrectSoapActionForMalformedSoapActionHeader()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" },
                { "SOAPAction", "Calculate" }
            });

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetSoapActionHeader" && x.IsPrivate)
                .First();

            var result = (string)method.Invoke(controller, new object[] { headers });
            
            Assert.Equal("Calculate", result);
        }

        [Fact]
        public void GetSoapActionHeader_ReturnsSanitizedValueFromProperSoapActionHeader()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            IHeaderDictionary headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                { "Content-Type", "application/json" },
                { "SOAPAction", "http://tempuri.org/Calculate" }
            });

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetSoapActionHeader" && x.IsPrivate)
                .First();

            var result = (string)method.Invoke(controller, new object[] { headers });

            Assert.Equal("Calculate", result.ToString());
        }

        [Fact]
        public void GetRequestBodyAsync_Returns_Content_For_POST()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

             var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate)
                .First();

            string httpRequestMethod = "POST";
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
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

             var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate)
                .First();

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
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate)
                .First();

            Assert.ThrowsAsync<ArgumentNullException>(() => 
                (Task<string>)method.Invoke(controller, new object[] { "POST", null })
            );
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenMethod_Is_Null()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "GetRequestBodyAsync" && x.IsPrivate)
                .First();

            Assert.ThrowsAsync<ArgumentNullException>(() => 
                (Task<string>)method.Invoke(controller, new object[] { null, null })
            );
        }

        [Fact]
        public void CombinePaths_Throws_Exception_When_Param1_IsNull_Or_Whitespace()
        {
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "CombinePaths" && x.IsPrivate)
                .First();

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
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "CombinePaths" && x.IsPrivate)
                .First();

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
            var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);

            var type = typeof(BaseActionApiController);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "CombinePaths" && x.IsPrivate)
                .First();

            var result = (string)method.Invoke(controller, new object[] { "post", "comments" });
            Assert.Equal("post/comments", result);
            
            result = (string)method.Invoke(controller, new object[] { "http://address/posts/", "/comments" });

            Assert.Equal("http://address/posts/comments", result);
        }

            [Fact]
            public void IntercepKnownRouteAsync_ThrowsExceptionForNullParameters()
            {
                var controller = new RestController(loggerMock.Object, anonymousServiceClientMock.Object, authenticatedServiceClientMock.Object, cacheMock.Object);
                var service = new ServiceDTO();

                var type = typeof(BaseActionApiController);

                MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate)
                    .First();

                var task = (Task<bool>)method.Invoke(controller, new object[] { null, "/comments", service });

                Assert.ThrowsAsync<ArgumentNullException>(async () => await task);

                task = (Task<bool>)method.Invoke(controller, new object[] { null, "/comments", service });
            }
    }
}