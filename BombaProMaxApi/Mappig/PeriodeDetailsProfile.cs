using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class PeriodeDetailsProfile : Profile
    {
        public PeriodeDetailsProfile()
        {
            // Entity to DTO mapping
            CreateMap<PeriodeDetails, PeriodeDetailsDto>()
                .ForMember(d => d.PompeNumero,
                    opt => opt.MapFrom(s => s.Pompe != null ? s.Pompe.Numero : null))
                .ForMember(d => d.ReservoirNumero,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Numero : null))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null));

            // DTO to Entity mapping - ignore navigation properties
            CreateMap<PeriodeDetailsDto, PeriodeDetails>()
                .ForMember(d => d.Periode, opt => opt.Ignore())
                .ForMember(d => d.Pompe, opt => opt.Ignore())
                .ForMember(d => d.Reservoir, opt => opt.Ignore())
                .ForMember(d => d.Produit, opt => opt.Ignore());
        }
    }
}
