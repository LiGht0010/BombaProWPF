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
                // Map pricing from Product (HT, TVA, TTC)
                .ForMember(d => d.PrixHT,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.PrixHT : 
                                            s.Service != null ? s.Service.Prix : 
                                            s.PrixUnitaire))
                .ForMember(d => d.TVA,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.TVA : 
                                            s.Service != null ? (decimal?)20 : 
                                            (decimal?)20)) // Default 20% TVA
                .ForMember(d => d.PrixTTC,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.PrixTTC : 
                                            s.Service != null ? s.Service.Prix * 1.20m : 
                                            s.PrixUnitaire))
                .ReverseMap();
        }
    }
}
