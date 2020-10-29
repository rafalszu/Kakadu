using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AutoMapper;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
using Kakadu.Common.Extensions;

namespace Kakadu.ConfigurationApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IEntityDTO, IModel>()
                .ForMember(model => model.Id, opt => opt.MapFrom(dto => MapId(dto)));

            CreateMap<ServiceModel, ServiceDTO>()
                .ForMember(dto => dto.Endpoints, opt => opt.MapFrom<ActionEndpointsResolver>());
            CreateMap<ServiceDTO, ServiceModel>()
                .IncludeBase<IEntityDTO, IModel>();

            CreateMap<CreateServiceDTO, ServiceModel>()
                .IncludeBase<IEntityDTO, IModel>();

            CreateMap<UserModel, UserDTO>();
            CreateMap<UserDTO, UserModel>()
                .IncludeBase<IEntityDTO,IModel>();            
                
            CreateMap<KnownRouteReplyModel, KnownRouteReplyDTO>();
            CreateMap<KnownRouteReplyDTO, KnownRouteReplyModel>()
                .IncludeBase<IEntityDTO, IModel>()
                .ForMember(model => model.ContentBase64, opt => opt.MapFrom<HttpContentResolver>())
                ;

            CreateMap<KnownRouteModel, KnownRouteDTO>()
                .ForMember(dto => dto.MethodName, opt => opt.MapFrom(model => model.Method.ToString()));
            CreateMap<KnownRouteDTO, KnownRouteModel>()
                .IncludeBase<IEntityDTO, IModel>()
                .ForMember(model => model.Method, opt => opt.MapFrom((dto, model) =>
                {
                    if (!Enum.TryParse<MethodTypeEnum>(dto.MethodName, true, out MethodTypeEnum result))
                        return MethodTypeEnum.Uknown;
                    return result;
                }));
        }

        Guid MapId(IEntityDTO dto)
        {
            if (dto is null || dto.Id == Guid.Empty)
                return Guid.NewGuid();

            return dto.Id;
        }
    }

    public class ActionEndpointsResolver : IValueResolver<ServiceModel, ServiceDTO, List<string>>
    {
        private readonly IDistributedCache _cache;

        public ActionEndpointsResolver(IDistributedCache cache)
        {
            _cache = cache;
        }
        
        public List<string> Resolve(ServiceModel source, ServiceDTO destination, List<string> destMember, ResolutionContext context)
        {
            var instances = _cache.Get<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if (instances == null || !instances.Any())
                return new List<string>();

            return instances.Select(instance => ConcatUrl(instance, source.Code)).ToList();
        }

        private string ConcatUrl(string baseUrl, string code) =>
            string.Format("{0}{1}rest/{2}",
                baseUrl,
                baseUrl.EndsWith('/') ? "" : "/",
                code);

    }

    public class HttpContentResolver : IValueResolver<KnownRouteReplyDTO, KnownRouteReplyModel, string>
    {
        public string Resolve(KnownRouteReplyDTO source, KnownRouteReplyModel destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return string.Empty;

            // decompress content if needed
            return string.IsNullOrWhiteSpace(source.ContentEncoding) ? source.ContentBase64 : DecompressResponse(source);
        }

        private string DecompressResponse(KnownRouteReplyDTO dto)
        {
            if (dto == null)
                return string.Empty;

            if (dto.ContentEncoding.Equals("br", StringComparison.InvariantCultureIgnoreCase))
                return Decompress(dto.ContentBase64, memoryStream => new BrotliStream(memoryStream, CompressionMode.Decompress, false));
            if (dto.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                return Decompress(dto.ContentBase64, memoryStream => new GZipStream(memoryStream, CompressionMode.Decompress, false));
            if (dto.ContentEncoding.Equals("deflate", StringComparison.InvariantCultureIgnoreCase))
                return Decompress(dto.ContentBase64, memoryStream => new DeflateStream(memoryStream, CompressionMode.Decompress, false));
            
            return dto.ContentBase64;
        }

        private string Decompress(string base64, Func<Stream, Stream> streamFunc)
        {
            if (string.IsNullOrWhiteSpace(base64) || streamFunc == null)
                return string.Empty;

            using var outputStream = new MemoryStream();
            using var entry = new MemoryStream(Convert.FromBase64String(base64));
            using var stream = streamFunc(entry);
            
            var buffer = new byte[1024];
            int nRead;
            while ((nRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                outputStream.Write(buffer, 0, nRead);

            return Convert.ToBase64String(outputStream.ToArray());
        }
    }
}