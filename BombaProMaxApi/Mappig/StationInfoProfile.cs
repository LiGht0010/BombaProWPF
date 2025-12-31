using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class StationInfoProfile : Profile
{
    public StationInfoProfile()
    {
        CreateMap<StationInfo, StationInfoDto>()
            .ForMember(dest => dest.LogoBase64, opt => opt.MapFrom(src => 
                src.Logo != null ? Convert.ToBase64String(src.Logo) : null));
        
        CreateMap<StationInfoDto, StationInfo>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.LogoBase64) ? Convert.FromBase64String(src.LogoBase64) : null));
    }
}
