using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class PeriodeProfile : Profile
    {
        public PeriodeProfile()
        {
            // Entity to DTO mapping
            CreateMap<Periode, PeriodeDto>()
                .ForMember(d => d.EmployeNom,
                    opt => opt.MapFrom(s => s.Employe != null ? s.Employe.Nom : null));

            // DTO to Entity mapping - ignore navigation properties
            CreateMap<PeriodeDto, Periode>()
                .ForMember(d => d.Employe, opt => opt.Ignore())
                .ForMember(d => d.PeriodeDetails, opt => opt.Ignore());
        }
    }
}
