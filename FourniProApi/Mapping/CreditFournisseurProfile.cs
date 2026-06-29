using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class CreditFournisseurProfile : Profile
{
    public CreditFournisseurProfile()
    {
        CreateMap<CreditFournisseur, CreditFournisseurDto>()
            .ForMember(d => d.AchatNumero,    opt => opt.Ignore())
            .ForMember(d => d.FournisseurNom, opt => opt.Ignore())
            .ForMember(d => d.EmployeNom,     opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,   opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom,  opt => opt.Ignore());

        CreateMap<CreditFournisseurDto, CreditFournisseur>()
            .ForMember(d => d.CreditFournisseurId,    opt => opt.Ignore())
            .ForMember(d => d.Achat,                  opt => opt.Ignore())
            .ForMember(d => d.Fournisseur,            opt => opt.Ignore())
            .ForMember(d => d.Employe,                opt => opt.Ignore())
            .ForMember(d => d.PaiementsFournisseur,   opt => opt.Ignore());
    }
}
