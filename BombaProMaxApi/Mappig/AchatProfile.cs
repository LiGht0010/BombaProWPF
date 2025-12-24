using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class AchatProfile : Profile
    {
        public AchatProfile()
        {
            // Entity to DTO mapping
            CreateMap<Achat, AchatDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Nom : null))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.ChauffeurNom,
                    opt => opt.MapFrom(s => s.Chauffeur != null ? s.Chauffeur.Nom : null))
                .ForMember(d => d.CamionImmatriculation,
                    opt => opt.MapFrom(s => s.Camion != null ? s.Camion.Matricule : null));

            // DTO to Entity mapping - ignore display-only fields
            CreateMap<AchatDto, Achat>()
                .ForMember(d => d.Fournisseur, opt => opt.Ignore())
                .ForMember(d => d.Produit, opt => opt.Ignore())
                .ForMember(d => d.Chauffeur, opt => opt.Ignore())
                .ForMember(d => d.Camion, opt => opt.Ignore())
                .ForMember(d => d.AchatAllocations, opt => opt.Ignore());
        }
    }
}