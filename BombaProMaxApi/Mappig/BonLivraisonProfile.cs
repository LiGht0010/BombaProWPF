using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class BonLivraisonProfile : Profile
{
    public BonLivraisonProfile()
    {
        CreateMap<BonLivraison, BonLivraisonDto>()
            .ForMember(d => d.ClientNom,
                opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
            .ForMember(d => d.ClientNumero,
                opt => opt.MapFrom(s => s.Client != null ? s.Client.NumeroClient : null))
            .ForMember(d => d.Details,
                opt => opt.MapFrom(s => s.Details));

        CreateMap<BonLivraisonDto, BonLivraison>();

        CreateMap<CreateBonLivraisonDto, BonLivraison>()
            .ForMember(d => d.ID, opt => opt.Ignore())
            .ForMember(d => d.MontantTotal, opt => opt.Ignore())
            .ForMember(d => d.EstFacture, opt => opt.Ignore())
            .ForMember(d => d.DateCreation, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Details, opt => opt.Ignore());

        CreateMap<UpdateBonLivraisonDto, BonLivraison>()
            .ForMember(d => d.MontantTotal, opt => opt.Ignore())
            .ForMember(d => d.EstFacture, opt => opt.Ignore())
            .ForMember(d => d.DateModification, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Details, opt => opt.Ignore());
    }
}
