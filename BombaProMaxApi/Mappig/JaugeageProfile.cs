using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class JaugeageProfile : Profile
    {
        public JaugeageProfile()
        {
            // Map from Jaugeage entity to JaugeageDto
            CreateMap<Jaugeage, JaugeageDto>()
                .ForMember(d => d.TemoinNom,
                    opt => opt.MapFrom(s => s.Temoin != null ? s.Temoin.Nom : null));

            // Map from JaugeageDto to Jaugeage entity - ignore navigation properties
            CreateMap<JaugeageDto, Jaugeage>()
                .ForMember(d => d.Temoin, opt => opt.Ignore())
                .ForMember(d => d.JaugeageDetails, opt => opt.Ignore());
        }
    }
}
