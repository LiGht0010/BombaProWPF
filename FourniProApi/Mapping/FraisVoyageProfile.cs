using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class FraisVoyageProfile : Profile
{
    public FraisVoyageProfile()
    {
        CreateMap<FraisVoyage, FraisVoyageDto>();

        CreateMap<FraisVoyageDto, FraisVoyage>()
            .ForMember(d => d.FraisVoyageId, opt => opt.Ignore())
            .ForMember(d => d.Voyage,        opt => opt.Ignore());
    }
}
