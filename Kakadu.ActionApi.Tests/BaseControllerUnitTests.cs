using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Kakadu.ActionApi.Controllers;
using Xunit;

namespace Kakadu.ActionApi.Tests
{
    public class BaseControllerUnitTests
    {
        [Fact]
        public void GetRequestBodyAsync_Returns_Content_For_POST()
        {
            BaseActionApiController controller = new BaseActionApiController(null, null, null, null);

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


            string content = controller.GetRequestBodyAsync(httpRequestMethod, body).GetAwaiter().GetResult();
        
            Assert.NotNull(content);

            Assert.NotEmpty(content);
        }

        [Fact]
        public void GetRequestBodyAsync_Returns_EmptyString_For_GET()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

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


            string content = controller.GetRequestBodyAsync(httpRequestMethod, body).GetAwaiter().GetResult();
        
            Assert.NotNull(content);

            Assert.Empty(content);
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenStream_Is_Null()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

            Assert.ThrowsAsync<ArgumentNullException>(() => controller.GetRequestBodyAsync("POST", null));
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenMethod_Is_Null()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

            Assert.ThrowsAsync<ArgumentNullException>(() => controller.GetRequestBodyAsync(null, null));
        }

        [Fact]
        public void CombinePaths_Throws_Exception_When_Param1_IsNull_Or_Whitespace()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

            Assert.Throws<Exception>(() => controller.CombinePaths(null, "abc"));

            Assert.Throws<Exception>(() => controller.CombinePaths("", "abc"));

            Assert.Throws<Exception>(() => controller.CombinePaths(" ", "abc"));
        }

        [Fact]
        public void CombinePaths_Throws_Exception_When_Param2_IsNull_Or_Whitespace()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

            Assert.Throws<Exception>(() => controller.CombinePaths("abc", null));

            Assert.Throws<Exception>(() => controller.CombinePaths("abc", ""));

            Assert.Throws<Exception>(() => controller.CombinePaths("abc", " "));
        }

        [Fact]
        public void CombinePaths_ReturnsCorrectPath()
        {
            BaseActionApiController controller = new BaseActionApiController(null);

            Assert.Equal("post/comments", controller.CombinePaths("post", "comments"));

            Assert.Equal("http://address/posts/comments", controller.CombinePaths("http://address/posts/", "/comments"));
        }


    }
}