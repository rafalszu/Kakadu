using AutoMapper;
using Kakadu.Core.Models;
using Kakadu.DTO;

namespace Kakadu.WebApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ServiceModel, ServiceDTO>();
            CreateMap<ServiceDTO, ServiceModel>();
        }
    }
}