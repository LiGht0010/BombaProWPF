using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ReservoirProfile : Profile
    {
        public ReservoirProfile()
        {
            CreateMap<Reservoir, ReservoirDto>()
                .ForMember(d => d.ProduitNom,
                    opt => opt.MapFrom(s => s.Produit != null ? s.Produit.Description : null))
                .ForMember(d => d.HasCalibration,
                    opt => opt.MapFrom(s => s.Calibrations != null && s.Calibrations.Count > 0))
                .ForMember(d => d.CalibrationCount,
                    opt => opt.MapFrom(s => s.Calibrations != null ? s.Calibrations.Count : 0))
                .ReverseMap()
                .ForMember(d => d.Calibrations, opt => opt.Ignore());
        }
    }
}
