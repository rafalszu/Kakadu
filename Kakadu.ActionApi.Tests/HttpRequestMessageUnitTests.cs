using System.Net.Http;
using Kakadu.ActionApi.Extensions;
using Xunit;


namespace Kakadu.ActionApi.Tests
{
    public class HttpRequestMessageUnitTests
    {
        [Fact]
        public void Returns_SoapAction_Value_For_POST()
        {
            HttpRequestMessage msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { { "SOAPAction", "http://tempuri.org/Calculate" } }
            };

            string action = msg.GetActionHeaderValue();
            
            Assert.NotNull(action);

            Assert.NotEmpty(action);

            Assert.Equal("Calculate", action);
        }

        [Fact]
        public void Returns_SoapAction_Value_For_POST_With_Malofrmed_SOAPAction_HeaderValue()
        {
            HttpRequestMessage msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { { "SOAPAction", "Calculate" } }
            };

            string action = msg.GetActionHeaderValue();

            Assert.NotNull(action);

            Assert.NotEmpty(action);

            Assert.Equal("Calculate", action);
        }

        [Fact]
        public void Returns_SoapAction_Value_For_POST_With_Malofrmed_SOAPAction_HeaderValue2()
        {
            HttpRequestMessage msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { { "SOAPAction", "http://tempuri.org/\\Calculate\"\\" } }
            };

            string action = msg.GetActionHeaderValue();

            Assert.NotNull(action);

            Assert.NotEmpty(action);

            Assert.Equal("Calculate", action);
        }

        [Fact]
        public void Returns_SoapAction_Empty_For_GET()
        {
            HttpRequestMessage msg = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers = { { "SOAPAction", "http://tempuri.org/Calculate" } }
            };

            string action = msg.GetActionHeaderValue();

            Assert.NotNull(action);

            Assert.Empty(action);
        }

        [Fact]
        public void Returns_SoapAction_Empty_When_No_SOAPAction_Header()
        {
            HttpRequestMessage msg = new HttpRequestMessage
            {
                Method = HttpMethod.Get
            };

            string action = msg.GetActionHeaderValue();

            Assert.NotNull(action);

            Assert.Empty(action);
        }
    }
}