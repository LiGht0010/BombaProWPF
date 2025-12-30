using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class JaugeageDetailProfile : Profile
    {
        public JaugeageDetailProfile()
        {
            // Map from JaugeageDetail entity to JaugeageDetailDto
            CreateMap<JaugeageDetail, JaugeageDetailDto>()
                .ForMember(d => d.JaugeageNumero,
                    opt => opt.MapFrom(s => s.Jaugeage != null ? s.Jaugeage.NumeroJaugeage : null))
                .ForMember(d => d.ReservoirNumero,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Numero : null));

            // Map from JaugeageDetailDto to JaugeageDetail entity - ignore navigation properties
            CreateMap<JaugeageDetailDto, JaugeageDetail>()
                .ForMember(d => d.Jaugeage, opt => opt.Ignore())
                .ForMember(d => d.Reservoir, opt => opt.Ignore());
        }
    }
}
