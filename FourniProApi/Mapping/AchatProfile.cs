using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class AchatProfile : Profile
{
    public AchatProfile()
    {
        CreateMap<Achat, AchatDto>()
            .ForMember(d => d.FournisseurNom, opt => opt.Ignore())
            .ForMember(d => d.ProduitNom, opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom, opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<AchatDto, Achat>()
            .ForMember(d => d.AchatId, opt => opt.Ignore());
    }
}
