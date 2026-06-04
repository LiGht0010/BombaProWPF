using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class FournisseurProfile : Profile
{
    public FournisseurProfile()
    {
        // Entity → DTO (AjouteParNom / ModifieParNom are filled by the controller)
        CreateMap<Fournisseur, FournisseurDto>()
            .ForMember(dest => dest.AjouteParNom, opt => opt.Ignore())
            .ForMember(dest => dest.ModifieParNom, opt => opt.Ignore());

        // DTO → Entity (ignore resolved names and PK on insert)
        CreateMap<FournisseurDto, Fournisseur>()
            .ForMember(dest => dest.FournisseurId, opt => opt.Ignore())
            .ForMember(dest => dest.AjoutePar, opt => opt.MapFrom(src => src.AjoutePar))
            .ForMember(dest => dest.ModifiePar, opt => opt.MapFrom(src => src.ModifiePar));
    }
}
