using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class FactureProfile : Profile
    {
        public FactureProfile()
        {
            CreateMap<Facture, FactureDto>()
                .ForMember(d => d.ClientNom,
                    opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
                .ForMember(d => d.MoyenPaiementNom,
                    opt => opt.MapFrom(s => s.MoyenPaiement != null ? s.MoyenPaiement.Nom : null))
                .ReverseMap();
        }
    }
}
