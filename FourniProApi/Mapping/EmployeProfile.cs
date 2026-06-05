using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class EmployeProfile : Profile
{
    public EmployeProfile()
    {
        CreateMap<Employe, EmployeDto>()
            .ForMember(d => d.AjouteParNom,  opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<EmployeDto, Employe>()
            .ForMember(d => d.EmployeId, opt => opt.Ignore())
            .ForMember(d => d.Achats,    opt => opt.Ignore())
            .ForMember(d => d.Ventes,    opt => opt.Ignore());
    }
}
