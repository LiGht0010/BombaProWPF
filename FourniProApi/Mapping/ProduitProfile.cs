using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class ProduitProfile : Profile
{
    public ProduitProfile()
    {
        // Produit → ProduitDto: map computed fields from the model helpers
        CreateMap<Produit, ProduitDto>()
            .ForMember(dest => dest.MargeBeneficiaire, opt => opt.MapFrom(src => src.MargeBeneficiaire))
            .ForMember(dest => dest.MargePourcentage, opt => opt.MapFrom(src => src.MargePourcentage));

        // ProduitDto → Produit: ignore computed fields and ProduitId (set by DB on insert)
        CreateMap<ProduitDto, Produit>()
            .ForMember(dest => dest.ProduitId, opt => opt.Ignore())
            .ForMember(dest => dest.MargeBeneficiaire, opt => opt.Ignore())
            .ForMember(dest => dest.MargePourcentage, opt => opt.Ignore());
    }
}
