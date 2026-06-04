using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        // Entity → DTO (AjouteParNom / ModifieParNom are filled by the controller)
        CreateMap<Client, ClientDto>()
            .ForMember(dest => dest.AjouteParNom, opt => opt.Ignore())
            .ForMember(dest => dest.ModifieParNom, opt => opt.Ignore());

        // DTO → Entity (ignore resolved names and PK on insert)
        CreateMap<ClientDto, Client>()
            .ForMember(dest => dest.ClientId, opt => opt.Ignore())
            .ForMember(dest => dest.AjoutePar, opt => opt.MapFrom(src => src.AjoutePar))
            .ForMember(dest => dest.ModifiePar, opt => opt.MapFrom(src => src.ModifiePar));
    }
}
