using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class JaugeageProfile : Profile
    {
        public JaugeageProfile()
        {
            CreateMap<Jaugeage, JaugeageDto>()
                .ForMember(d => d.TemoinNom,
                    opt => opt.MapFrom(s => s.Temoin != null ? s.Temoin.Nom : null))
                .ReverseMap();
        }
    }
}
