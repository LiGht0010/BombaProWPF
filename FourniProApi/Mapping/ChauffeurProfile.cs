using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class ChauffeurProfile : Profile
{
    public ChauffeurProfile()
    {
        CreateMap<Chauffeur, ChauffeurDto>()
            .ForMember(d => d.AjouteParNom, opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<ChauffeurDto, Chauffeur>()
            .ForMember(d => d.ChauffeurId, opt => opt.Ignore());
    }
}
