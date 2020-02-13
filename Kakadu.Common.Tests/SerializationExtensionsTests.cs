using System;
using Xunit;
using FluentAssertions;
using Kakadu.Common.Extensions;
using Kakadu.DTO;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Kakadu.Common.Tests
{
    public class SerializationExtensionsTests
    {
        [Fact]
        public void Serialization_SupportString()
        {
            string value = "test value";
            byte[] serialized = value.ToByteArray();
            
            serialized
                .Should()
                .NotBeNullOrEmpty();

            string result = serialized.FromByteArray();
            result.
                Should()
                .NotBeNullOrWhiteSpace()
                .And
                .BeEquivalentTo(value);
        }

        [Fact]
        public void Serialization_SupportBool()
        {
            bool value = true;
            byte[] serialized = value.ToByteArray();

            serialized
                .Should()
                .NotBeNullOrEmpty();

            bool result = serialized.FromByteArray<bool>();
            result
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Serialization_SupportsInt()
        {
            int value = 124;
            byte[] serialized = value.ToByteArray();

            serialized
                .Should()
                .NotBeNullOrEmpty();

            int result = serialized.FromByteArray<int>();
            result
                .Should()
                .Be(124);
        }

        [Fact]
        public void Serialization_SupportsDecimal()
        {
            decimal value = 12m;
            byte[] serialized = value.ToByteArray();

            serialized
                .Should()
                .NotBeNullOrEmpty();

            decimal result = serialized.FromByteArray<decimal>();
            result
                .Should()
                .Be(12m);
        }

        [Fact]
        public void Serialization_SupportsFloat()
        {
            float value = 56.1f;
            byte[] serialized = value.ToByteArray();

            serialized
                .Should()
                .NotBeNullOrEmpty();

            float result = serialized.FromByteArray<float>();
            result
                .Should()
                .Be(56.1f);
        }

        [Fact]
        public void Serialization_SupportsDouble()
        {
            double value = 442.21d;
            byte[] serialized = value.ToByteArray();
            serialized
                .Should()
                .NotBeNullOrEmpty();

            double result = serialized.FromByteArray<double>();
            result
                .Should()
                .Be(442.21d);
        }

        [Fact]
        public void Serialization_SupportsComplexObject()
        {
            ServiceDTO dto = new ServiceDTO {
                Id = Guid.NewGuid(),
                Code = "dummy"
            };

            byte[] serialized = dto.ToByteArray();
            serialized
                .Should()
                .NotBeNullOrEmpty();

            var result = serialized.FromByteArray<ServiceDTO>();
            result
                .Should()
                .NotBeNull()
                .And
                .BeEquivalentTo<ServiceDTO>(dto);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SetAsync_ThrowsExceptionWhenKeyIsNullOrEmpty(string key)
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            Func<Task> func = () => cacheMock.Object.SetAsync<byte[]>(key, 123.ToByteArray(), null, cts.Token);

            await func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAsync_ThrowsExceptionWhenKeyIsNullOrEmpty(string key)
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            Func<Task> func = () => cacheMock.Object.GetAsync<byte[]>(key, cts.Token);

            await func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetOrAddStringAsync_ThrowsExceptionWhenKeyIsNullOrEmpty(string key)
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            Func<Task> func = () => cacheMock.Object.GetOrAddStringAsync(key, (options) => {
                return Task.FromResult("abc");
            }, cts.Token);

            await func.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetOrAddStringAsync_ReturnsStringItemFromCacheIfPresent()
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes("cache value");

            cacheMock.Setup(x => 
                x.Get("dummy")
            ).Returns(valueBytes);

            string result = await cacheMock.Object.GetOrAddStringAsync("dummy", (options) => {
                return Task.FromResult("value returned from factory");
            }, cts.Token);

            result
                .Should()
                .NotBeNullOrWhiteSpace()
                .And
                .BeEquivalentTo("cache value");
        }

        [Fact]
        public async Task GetOrAddStringAsync_ReturnsStringItemFromDelegateIfNotPresentInCache()
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            byte[] emptyBytes = null;

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(emptyBytes);

            string result = await cacheMock.Object.GetOrAddStringAsync("dummy", (options) => {
                return Task.FromResult("important value");
            }, cts.Token);

            result.Should()
                .NotBeNullOrWhiteSpace()
                .And
                .BeEquivalentTo("important value");
        }

        [Fact]
        public async Task GetOrAddAsync_ReturnsComplexItemFromCacheIfPresent()
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            var service = new ServiceDTO { Id = Guid.NewGuid(), Code = "dummyservice" };
            byte[] valueBytes = service.ToByteArray();

            cacheMock.Setup(x => 
                x.GetAsync("dummy", It.IsAny<CancellationToken>())
            ).ReturnsAsync(valueBytes);

            var result = await cacheMock.Object.GetOrAddAsync<ServiceDTO>("dummy", (options) => {
                return Task.FromResult(new ServiceDTO { Id = Guid.NewGuid(), Code = "otherservice" });
            }, cts.Token);

            result
                .Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(service);
        }

        [Fact]
        public async Task GetOrAddAsync_ReturnsComplexItemFromDelegateIfNotPresentInCache()
        {
            Mock<IDistributedCache> cacheMock = new Mock<IDistributedCache>();
            CancellationTokenSource cts = new CancellationTokenSource(1000);

            byte[] emptyBytes = null;

            cacheMock.Setup(x => 
                x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync(emptyBytes);

            ServiceDTO dto = new ServiceDTO { Id = Guid.NewGuid(), Code = "service1" };

            var result = await cacheMock.Object.GetOrAddAsync<ServiceDTO>("dummy", (options) => {
                return Task.FromResult(dto);
            }, cts.Token);

            result.Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(dto);
        }
    }
}
