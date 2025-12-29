using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ProduitProfile : Profile
    {
        public ProduitProfile()
        {
            // Entity to DTO mapping
            CreateMap<Produit, ProduitDto>()
                .ForMember(d => d.CategorieNom,
                    opt => opt.MapFrom(s => s.Categorie != null ? s.Categorie.Nom : null));

            // DTO to Entity mapping - ignore navigation property to prevent creating new categories
            CreateMap<ProduitDto, Produit>()
                .ForMember(d => d.Categorie, opt => opt.Ignore())
                .ForMember(d => d.Achats, opt => opt.Ignore())
                .ForMember(d => d.CreditTransactions, opt => opt.Ignore())
                .ForMember(d => d.ElementsFactures, opt => opt.Ignore())
                .ForMember(d => d.Reservoirs, opt => opt.Ignore())
                .ForMember(d => d.VentesLubrifiantsEtArticles, opt => opt.Ignore())
                .ForMember(d => d.BonLivraisonDetails, opt => opt.Ignore());
        }
    }
}
