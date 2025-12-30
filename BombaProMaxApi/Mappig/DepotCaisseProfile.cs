using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

/// <summary>
/// AutoMapper profile for DepotCaisse entity.
/// </summary>
public class DepotCaisseProfile : Profile
{
    public DepotCaisseProfile()
    {
        // Entity to DTO
        CreateMap<DepotCaisse, DepotCaisseDto>()
            .ForMember(dest => dest.ValidateurNom, opt => opt.MapFrom(src => src.Validateur != null ? src.Validateur.Name : null));

        // DTO to Entity
        CreateMap<DepotCaisseDto, DepotCaisse>()
            .ForMember(dest => dest.Validateur, opt => opt.Ignore());
    }
}
