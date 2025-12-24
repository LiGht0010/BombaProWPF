using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CreditTransactionProfile : Profile
    {
        public CreditTransactionProfile()
        {
            // Entity to DTO mapping
            CreateMap<CreditTransaction, CreditTransactionDto>()
                .ForMember(d => d.ClientNom,
                    opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.ServiceNom,
                    opt => opt.MapFrom(s => s.Service != null ? s.Service.Description : null))
                .ForMember(d => d.FactureNumero,
                    opt => opt.MapFrom(s => s.FactureAssociee != null ? s.FactureAssociee.NumeroFacture : null))
                .ForMember(d => d.BonLivraisonNumero,
                    opt => opt.MapFrom(s => s.BonLivraison != null ? s.BonLivraison.NumeroBL : null))
                .ForMember(d => d.IsSelected, opt => opt.Ignore()); // UI-only field

            // DTO to Entity mapping - ignore display-only and UI fields
            CreateMap<CreditTransactionDto, CreditTransaction>()
                .ForMember(d => d.Client, opt => opt.Ignore())
                .ForMember(d => d.Produit, opt => opt.Ignore())
                .ForMember(d => d.Service, opt => opt.Ignore())
                .ForMember(d => d.FactureAssociee, opt => opt.Ignore())
                .ForMember(d => d.BonLivraison, opt => opt.Ignore());
        }
    }
}
