using AutoMapper;
using BombaProMaxApi.Models;
using BombaProMaxApi.DTOs;

public class PompeProfile : Profile
{
    public PompeProfile()
    {
        CreateMap<Pompe, PompeDto>()
            .ForMember(d => d.ReservoirNumero,
                opt => opt.MapFrom(s => s.ReservoirAssocie != null ? s.ReservoirAssocie.Numero : null))
            .ForMember(d => d.ReservoirCapacite,
                opt => opt.MapFrom(s => s.ReservoirAssocie != null ? s.ReservoirAssocie.Capacite : (decimal?)null))
            .ForMember(d => d.ReservoirNiveauDeCarburant,
                opt => opt.MapFrom(s => s.ReservoirAssocie != null ? s.ReservoirAssocie.NiveauDeCarburant : (decimal?)null))
            .ForMember(d => d.CarburantNom,
                opt => opt.MapFrom(s => s.ReservoirAssocie != null && s.ReservoirAssocie.Produit != null 
                    ? s.ReservoirAssocie.Produit.Description : null))
            .ReverseMap()
            .ForMember(d => d.ReservoirAssocie, opt => opt.Ignore());
    }
}
