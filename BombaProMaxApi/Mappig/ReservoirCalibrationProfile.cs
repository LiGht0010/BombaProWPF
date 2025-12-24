using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class ReservoirCalibrationProfile : Profile
{
    public ReservoirCalibrationProfile()
    {
        CreateMap<ReservoirCalibration, ReservoirCalibrationDto>()
            .ForMember(d => d.ReservoirNumero,
                opt => opt.MapFrom(s => s.Reservoir != null ? s.Reservoir.Numero : null))
            .ReverseMap();

        CreateMap<ReservoirCalibrationImportDto, ReservoirCalibration>();
    }
}
