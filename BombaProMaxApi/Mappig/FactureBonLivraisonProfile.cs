using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class FactureBonLivraisonProfile : Profile
{
    public FactureBonLivraisonProfile()
    {
        CreateMap<FactureBonLivraison, FactureBonLivraisonDto>()
            .ForMember(d => d.FactureNumero,
                opt => opt.MapFrom(s => s.Facture != null ? s.Facture.NumeroFacture : null))
            .ForMember(d => d.BonLivraisonNumero,
                opt => opt.MapFrom(s => s.BonLivraison != null ? s.BonLivraison.NumeroBL : null))
            .ForMember(d => d.BonLivraisonDate,
                opt => opt.MapFrom(s => s.BonLivraison != null ? s.BonLivraison.DateBL : (DateOnly?)null))
            .ForMember(d => d.BonLivraisonMontant,
                opt => opt.MapFrom(s => s.BonLivraison != null ? s.BonLivraison.MontantTotal : (decimal?)null));

        CreateMap<FactureBonLivraisonDto, FactureBonLivraison>();
    }
}
