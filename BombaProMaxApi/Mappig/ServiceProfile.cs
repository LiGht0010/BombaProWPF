using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<Service, ServiceDto>()
                .ReverseMap();
        }
    }
}
