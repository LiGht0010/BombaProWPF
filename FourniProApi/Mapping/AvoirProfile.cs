using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class AvoirProfile : Profile
{
    public AvoirProfile()
    {
        CreateMap<Avoir, AvoirDto>()
            .ForMember(d => d.NumeroVente,   opt => opt.Ignore())
            .ForMember(d => d.NumeroCredit,  opt => opt.Ignore())
            .ForMember(d => d.ProduitNom,    opt => opt.Ignore())
            .ForMember(d => d.ClientNom,     opt => opt.Ignore())
            .ForMember(d => d.EmployeNom,    opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,  opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<AvoirDto, Avoir>()
            .ForMember(d => d.AvoirId,       opt => opt.Ignore())
            .ForMember(d => d.Vente,         opt => opt.Ignore())
            .ForMember(d => d.Credit,        opt => opt.Ignore())
            .ForMember(d => d.Employe,       opt => opt.Ignore());
    }
}
