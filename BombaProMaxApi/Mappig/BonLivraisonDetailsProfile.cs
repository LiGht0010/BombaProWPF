using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class BonLivraisonDetailsProfile : Profile
{
    public BonLivraisonDetailsProfile()
    {
        CreateMap<BonLivraisonDetails, BonLivraisonDetailsDto>()
            .ForMember(d => d.ProduitNom,
                opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
            .ForMember(d => d.ProduitNumero,
                opt => opt.MapFrom(s => s.Produit != null ? s.Produit.NumeroProduit : null))
            .ForMember(d => d.ServiceNom,
                opt => opt.MapFrom(s => s.Service != null ? s.Service.Description : null))
            .ForMember(d => d.ServiceNumero,
                opt => opt.MapFrom(s => s.Service != null ? s.Service.Numero : null));

        CreateMap<BonLivraisonDetailsDto, BonLivraisonDetails>();

        CreateMap<CreateBonLivraisonDetailsDto, BonLivraisonDetails>()
            .ForMember(d => d.ID, opt => opt.Ignore())
            .ForMember(d => d.BonLivraisonID, opt => opt.Ignore())
            .ForMember(d => d.MontantLigne, opt => opt.MapFrom(s => s.Quantite * s.PrixUnitaire));
    }
}
