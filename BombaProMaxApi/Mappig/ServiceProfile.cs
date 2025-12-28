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
                .ForMember(dest => dest.ServiceCategorieNom, 
                    opt => opt.MapFrom(src => src.ServiceCategorie != null ? src.ServiceCategorie.Nom : null))
                .ReverseMap()
                .ForMember(dest => dest.ServiceCategorie, opt => opt.Ignore());
        }
    }
}
