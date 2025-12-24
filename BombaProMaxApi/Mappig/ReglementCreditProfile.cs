using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ReglementCreditProfile : Profile
    {
        public ReglementCreditProfile()
        {
            // Entity to DTO mapping
            CreateMap<ReglementCredit, ReglementCreditDto>()
                .ForMember(d => d.ClientNom,
                    opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
                .ForMember(d => d.ModePaiementNom,
                    opt => opt.MapFrom(s => s.ModePaiement != null ? s.ModePaiement.Nom : null));

            // DTO to Entity mapping - ignore navigation properties
            CreateMap<ReglementCreditDto, ReglementCredit>()
                .ForMember(d => d.Client, opt => opt.Ignore())
                .ForMember(d => d.ModePaiement, opt => opt.Ignore());
        }
    }
}
