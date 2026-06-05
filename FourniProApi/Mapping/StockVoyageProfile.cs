using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class StockVoyageProfile : Profile
{
    public StockVoyageProfile()
    {
        CreateMap<StockVoyage, StockVoyageDto>()
            .ForMember(d => d.ProduitNom, opt => opt.Ignore());

        CreateMap<StockVoyageDto, StockVoyage>()
            .ForMember(d => d.StockVoyageId, opt => opt.Ignore())
            .ForMember(d => d.Voyage,        opt => opt.Ignore())
            .ForMember(d => d.Produit,       opt => opt.Ignore());
    }
}
