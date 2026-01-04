using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ReservoirProfile : Profile
    {
        public ReservoirProfile()
        {
            // Entity to DTO mapping
            CreateMap<Reservoir, ReservoirDto>()
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.HasCalibration,
                    opt => opt.MapFrom(s => s.Calibrations != null && s.Calibrations.Count > 0))
                .ForMember(d => d.CalibrationCount,
                    opt => opt.MapFrom(s => s.Calibrations != null ? s.Calibrations.Count : 0));

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<ReservoirDto, Reservoir>()
                .ForMember(d => d.Produit, opt => opt.Ignore())
                .ForMember(d => d.Pompes, opt => opt.Ignore())
                .ForMember(d => d.AchatAllocations, opt => opt.Ignore())
                .ForMember(d => d.JaugeageDetails, opt => opt.Ignore())
                .ForMember(d => d.StockLots, opt => opt.Ignore())
                .ForMember(d => d.Calibrations, opt => opt.Ignore());
        }
    }
}
