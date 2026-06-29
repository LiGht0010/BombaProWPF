using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class PaiementFournisseurProfile : Profile
{
    public PaiementFournisseurProfile()
    {
        CreateMap<PaiementFournisseur, PaiementFournisseurDto>()
            .ForMember(d => d.NumeroCreditF,  opt => opt.Ignore())
            .ForMember(d => d.EmployeNom,     opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,   opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom,  opt => opt.Ignore());

        CreateMap<PaiementFournisseurDto, PaiementFournisseur>()
            .ForMember(d => d.PaiementFournisseurId, opt => opt.Ignore())
            .ForMember(d => d.CreditFournisseur,     opt => opt.Ignore())
            .ForMember(d => d.Employe,               opt => opt.Ignore());
    }
}
