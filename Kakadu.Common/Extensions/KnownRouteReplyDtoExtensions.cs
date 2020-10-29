using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Kakadu.DTO;

namespace Kakadu.Common.Extensions
{
    public static class KnownRouteReplyDtoExtensions
    {
        private static readonly Dictionary<string, Type> supportedCompressions = new Dictionary<string, Type>
        {
            ["br"] = typeof(BrotliStream),
            ["gzip"] = typeof(GZipStream),
            ["deflate"] = typeof(DeflateStream)
        };
        
        public static string Compressed(this KnownRouteReplyDTO dto)
        {
            if (dto == null)
                return string.Empty;
            if (string.IsNullOrWhiteSpace(dto.ContentEncoding))
                return dto.ContentBase64;

            return supportedCompressions.ContainsKey(dto.ContentEncoding)
                ? CompressCore(dto.ContentBase64, memoryStream => CreateStream(memoryStream, CompressionMode.Compress, dto.ContentEncoding))
                : dto.ContentBase64;
        }
        
        public static string Decompressed(this KnownRouteReplyDTO dto)
        {
            if (dto == null)
                return string.Empty;
            if (string.IsNullOrWhiteSpace(dto.ContentEncoding))
                return dto.ContentBase64;

            return supportedCompressions.ContainsKey(dto.ContentEncoding)
                ? DecompressCore(dto.ContentBase64, memoryStream => CreateStream(memoryStream, CompressionMode.Decompress, dto.ContentEncoding))
                : dto.ContentBase64;
        }

        private static Stream CreateStream(Stream memoryStream, CompressionMode compressionMode, string encoding) =>
            (Stream) Activator.CreateInstance(supportedCompressions[encoding], memoryStream, compressionMode, true);

        private static string CompressCore(string inputBase64, Func<Stream, Stream> streamFunc)
        {
            if (string.IsNullOrWhiteSpace(inputBase64) || streamFunc == null)
                return string.Empty;
            
            using var uncompressedStream = new MemoryStream(Convert.FromBase64String(inputBase64));
            using var compressedStream = new MemoryStream();
                
            using var compressorStream = streamFunc(compressedStream);
            uncompressedStream.CopyTo(compressorStream);

            return Convert.ToBase64String(compressedStream.ToArray());
        }

        private static string DecompressCore(string inputBase64, Func<Stream, Stream> streamFunc)
        {
            if (string.IsNullOrWhiteSpace(inputBase64) || streamFunc == null)
                return string.Empty;

            var compressedStream = new MemoryStream(Convert.FromBase64String(inputBase64));

            using var decompressorStream = streamFunc(compressedStream);
            using var decompressedStream = new MemoryStream();
            decompressorStream.CopyTo(decompressedStream);

            return Convert.ToBase64String(decompressedStream.ToArray());
        }
    }
}