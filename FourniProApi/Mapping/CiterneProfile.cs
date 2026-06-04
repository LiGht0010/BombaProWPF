using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class CiterneProfile : Profile
{
    public CiterneProfile()
    {
        CreateMap<Citerne, CiterneDto>()
            .ForMember(d => d.AjouteParNom, opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom, opt => opt.Ignore());

        CreateMap<CiterneDto, Citerne>()
            .ForMember(d => d.CiterneId, opt => opt.Ignore());
    }
}
