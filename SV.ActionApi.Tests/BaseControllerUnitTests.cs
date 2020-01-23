using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using SV.ActionApi.Controllers;
using Xunit;

namespace SV.ActionApi.Tests
{
    public class BaseControllerUnitTests
    {
        [Fact]
        public void GetRequestBodyAsync_Returns_Content_For_POST()
        {
            BaseController controller = new BaseController(null);

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
            BaseController controller = new BaseController(null);

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
            BaseController controller = new BaseController(null);

            Assert.ThrowsAsync<ArgumentNullException>(() => controller.GetRequestBodyAsync("POST", null));
        }

        [Fact]
        public void GetRequestBodyAsync_ThrowsException_WhenMethod_Is_Null()
        {
            BaseController controller = new BaseController(null);

            Assert.ThrowsAsync<ArgumentNullException>(() => controller.GetRequestBodyAsync(null, null));
        }
    }
}