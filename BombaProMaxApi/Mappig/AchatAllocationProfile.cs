using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class AchatAllocationProfile : Profile
    {
        public AchatAllocationProfile()
        {
            // Entity to DTO mapping
            CreateMap<AchatAllocation, AchatAllocationDto>()
                .ForMember(d => d.AchatNumero,
                    opt => opt.MapFrom(s => s.Achat != null ? s.Achat.Numero : null))
                .ForMember(d => d.ReservoirNumero,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Numero : null))
                .ForMember(d => d.ReservoirCapacite,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Capacite : (decimal?)null))
                .ForMember(d => d.ReservoirNiveauActuel,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.NiveauDeCarburant : (decimal?)null))
                .ForMember(d => d.ProduitID,
                    opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.ProduitID : null))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Reservoir != null && s.Reservoir.Produit != null 
                        ? s.Reservoir.Produit.Description : null));

            // DTO to Entity mapping - ignore display-only fields
            CreateMap<AchatAllocationDto, AchatAllocation>()
                .ForMember(d => d.Achat, opt => opt.Ignore())
                .ForMember(d => d.Reservoir, opt => opt.Ignore());

            // Reservoir to ReservoirAllocationInfoDto mapping
            CreateMap<Reservoir, ReservoirAllocationInfoDto>()
                .ForMember(d => d.NiveauActuel,
                    opt => opt.MapFrom(s => s.NiveauDeCarburant))
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.EstCompatible, opt => opt.Ignore());
        }
    }
}
