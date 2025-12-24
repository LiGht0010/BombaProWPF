using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class VenteLubrifiantsEtArticlesProfile : Profile
    {
        public VenteLubrifiantsEtArticlesProfile()
        {
            // Entity to DTO mapping
            CreateMap<VenteLubrifiantsEtArticles, VenteLubrifiantsEtArticlesDto>()
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.ClientNom,
                    opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
                .ForMember(d => d.EmployeNom,
                    opt => opt.MapFrom(s => s.Employe != null ? s.Employe.Nom : null))
                .ForMember(d => d.MoyenPaiementNom,
                    opt => opt.MapFrom(s => s.MoyenPaiement != null ? s.MoyenPaiement.Nom : null))
                .ForMember(d => d.CategorieNom,
                    opt => opt.MapFrom(s => s.Produit != null && s.Produit.Categorie != null ? s.Produit.Categorie.Nom : "Non définie"));

            // DTO to Entity mapping (for Create/Update)
            CreateMap<VenteLubrifiantsEtArticlesDto, VenteLubrifiantsEtArticles>()
                .ForMember(d => d.ID, opt => opt.Ignore()) // Ignore ID for new records
                .ForMember(d => d.Produit, opt => opt.Ignore()) // Navigation properties
                .ForMember(d => d.Client, opt => opt.Ignore())
                .ForMember(d => d.Employe, opt => opt.Ignore())
                .ForMember(d => d.MoyenPaiement, opt => opt.Ignore());
        }
    }
}
