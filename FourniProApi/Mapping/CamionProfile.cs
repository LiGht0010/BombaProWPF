using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class CamionProfile : Profile
{
    public CamionProfile()
    {
        CreateMap<Camion, CamionDto>()
            .ForMember(d => d.AjouteParNom, opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<CamionDto, Camion>()
            .ForMember(d => d.CamionId, opt => opt.Ignore());
    }
}
