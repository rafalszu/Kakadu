using System;
using Xunit;
using FluentAssertions;
using Kakadu.Common.Extensions;
using Kakadu.DTO;

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
    }
}
