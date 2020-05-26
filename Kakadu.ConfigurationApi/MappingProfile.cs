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
                .ForMember(model => model.Id, opt => opt.MapFrom(dto => MapId(dto)));

            CreateMap<ServiceModel, ServiceDTO>();
            CreateMap<ServiceDTO, ServiceModel>()
                .IncludeBase<IEntityDTO, IModel>();

            CreateMap<CreateServiceDTO, ServiceModel>()
                .IncludeBase<IEntityDTO, IModel>();

            CreateMap<UserModel, UserDTO>();
            CreateMap<UserDTO, UserModel>()
                .IncludeBase<IEntityDTO,IModel>();            
                
            CreateMap<KnownRouteReplyModel, KnownRouteReplyDTO>();
            CreateMap<KnownRouteReplyDTO, KnownRouteReplyModel>()
                .IncludeBase<IEntityDTO, IModel>();

            CreateMap<KnownRouteModel, KnownRouteDTO>()
                .ForMember(dto => dto.MethodName, opt => opt.MapFrom(model => model.Method.ToString()));
            CreateMap<KnownRouteDTO, KnownRouteModel>()
                .IncludeBase<IEntityDTO, IModel>()
                .ForMember(model => model.Method, opt => opt.MapFrom((dto, model) => {
                    if(!Enum.TryParse<MethodTypeEnum>(dto.MethodName, true, out MethodTypeEnum result))
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
}