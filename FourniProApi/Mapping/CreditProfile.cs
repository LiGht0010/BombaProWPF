using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class CreditProfile : Profile
{
    public CreditProfile()
    {
        CreateMap<Credit, CreditDto>()
            .ForMember(d => d.ProduitNom,    opt => opt.Ignore())
            .ForMember(d => d.ClientNom,     opt => opt.Ignore())
            .ForMember(d => d.EmployeNom,    opt => opt.Ignore())
            .ForMember(d => d.VoyageNumero,  opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,  opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<CreditDto, Credit>()
            .ForMember(d => d.CreditId, opt => opt.Ignore())
            .ForMember(d => d.Voyage,   opt => opt.Ignore())
            .ForMember(d => d.Employe,  opt => opt.Ignore());
    }
}
