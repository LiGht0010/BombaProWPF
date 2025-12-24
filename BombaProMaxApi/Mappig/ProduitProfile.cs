using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ProduitProfile : Profile
    {
        public ProduitProfile()
        {
            CreateMap<Produit, ProduitDto>()
                .ForMember(d => d.CategorieNom,
                    opt => opt.MapFrom(s => s.Categorie != null ? s.Categorie.Nom : null))
                .ReverseMap();
        }
    }
}
