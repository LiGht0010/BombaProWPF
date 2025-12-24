using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class JaugeageDetailProfile : Profile
    {
        public JaugeageDetailProfile()
        {
            CreateMap<JaugeageDetail, JaugeageDetailDto>()
                .ForMember(d => d.JaugeageNumero,
                    opt => opt.MapFrom(s => s.Jaugeage != null ? s.Jaugeage.NumeroJaugeage : null))
                .ForMember(d => d.ReservoirNumero,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Numero : null))
                .ReverseMap();
        }
    }
}
