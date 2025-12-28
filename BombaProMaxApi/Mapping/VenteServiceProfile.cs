using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mapping;

public class VenteServiceProfile : Profile
{
    public VenteServiceProfile()
    {
        CreateMap<VenteService, VenteServiceDto>()
            .ForMember(dest => dest.ServiceNumero, opt => opt.MapFrom(src => src.Service != null ? src.Service.Numero : null))
            .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Service != null ? src.Service.Description : null))
            .ForMember(dest => dest.ServiceCategorieNom, opt => opt.MapFrom(src => src.Service != null && src.Service.ServiceCategorie != null ? src.Service.ServiceCategorie.Nom : null))
            .ForMember(dest => dest.ClientNom, opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.EmployeNom, opt => opt.MapFrom(src => src.Employe != null ? src.Employe.Nom : null))
            .ForMember(dest => dest.MoyenPaiementNom, opt => opt.MapFrom(src => src.MoyenPaiement != null ? src.MoyenPaiement.Nom : null))
            .ForMember(dest => dest.MontantTotal, opt => opt.MapFrom(src => src.MontantTotal));

        CreateMap<VenteServiceDto, VenteService>()
            .ForMember(dest => dest.Service, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.Employe, opt => opt.Ignore())
            .ForMember(dest => dest.MoyenPaiement, opt => opt.Ignore());
    }
}
