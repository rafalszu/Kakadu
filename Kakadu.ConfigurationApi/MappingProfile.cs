using System;
using AutoMapper;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using Kakadu.DTO;

namespace Kakadu.ConfigurationApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IEntityDTO, IModel>()
                .ForMember(model => model.Id, opt => opt.MapFrom(dto => dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id));

            CreateMap<ServiceModel, ServiceDTO>();
            CreateMap<ServiceDTO, ServiceModel>();

            CreateMap<UserModel, UserDTO>();
            CreateMap<UserDTO, UserModel>();
                
            CreateMap<KnownRouteReplyModel, KnownRouteReplyDTO>();
            CreateMap<KnownRouteReplyDTO, KnownRouteReplyModel>();

            CreateMap<KnownRouteModel, KnownRouteDTO>()
                .ForMember(dto => dto.MethodName, opt => opt.MapFrom(model => model.Method.ToString()));
            CreateMap<KnownRouteDTO, KnownRouteModel>()
                .ForMember(model => model.Method, opt => opt.MapFrom((dto, model) => {
                    if(!Enum.TryParse<MethodTypeEnum>(dto.MethodName, true, out MethodTypeEnum result))
                        return MethodTypeEnum.Uknown;
                    return result;
                }));
        }
    }
}