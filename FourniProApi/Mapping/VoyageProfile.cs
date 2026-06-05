using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class VoyageProfile : Profile
{
    public VoyageProfile()
    {
        CreateMap<Voyage, VoyageDto>()
            .ForMember(d => d.CamionMatricule,    opt => opt.Ignore())
            .ForMember(d => d.ChauffeurNom,       opt => opt.Ignore())
            .ForMember(d => d.CiterneMatricule,   opt => opt.Ignore())
            .ForMember(d => d.AjouteParNom,       opt => opt.Ignore())
            .ForMember(d => d.ModifieParNom,      opt => opt.Ignore());

        CreateMap<VoyageDto, Voyage>()
            .ForMember(d => d.VoyageId, opt => opt.Ignore())
            .ForMember(d => d.Camion,   opt => opt.Ignore())
            .ForMember(d => d.Chauffeur, opt => opt.Ignore())
            .ForMember(d => d.Citerne,  opt => opt.Ignore())
            .ForMember(d => d.Stocks,   opt => opt.Ignore())
            .ForMember(d => d.Frais,    opt => opt.Ignore());
    }
}
