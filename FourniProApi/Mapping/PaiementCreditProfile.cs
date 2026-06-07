using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class PaiementCreditProfile : Profile
{
    public PaiementCreditProfile()
    {
        CreateMap<PaiementCredit, PaiementCreditDto>()
            .ForMember(d => d.NumeroCredit, opt => opt.Ignore())
            .ForMember(d => d.EmployeNom,   opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,  opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<PaiementCreditDto, PaiementCredit>()
            .ForMember(d => d.PaiementCreditId, opt => opt.Ignore())
            .ForMember(d => d.Credit,           opt => opt.Ignore())
            .ForMember(d => d.Employe,          opt => opt.Ignore());
    }
}
