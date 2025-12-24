using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ElementsFactureProfile : Profile
    {
        public ElementsFactureProfile()
        {
            CreateMap<ElementsFacture, ElementsFactureDto>()
                .ForMember(d => d.FactureNumero,
                    opt => opt.MapFrom(s => s.Facture != null ? s.Facture.NumeroFacture : null))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.ServiceNom,
                    opt => opt.MapFrom(s => s.Service != null ? s.Service.Description : null))
                .ReverseMap();
        }
    }
}
