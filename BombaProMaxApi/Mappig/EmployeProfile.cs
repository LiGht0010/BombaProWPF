using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class EmployeProfile : Profile
    {
        public EmployeProfile()
        {
            // Entity to DTO mapping
            CreateMap<Employe, EmployeDto>();

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<EmployeDto, Employe>()
                .ForMember(d => d.Jaugeages, opt => opt.Ignore())
                .ForMember(d => d.EmployeBilanCredit, opt => opt.Ignore())
                .ForMember(d => d.EmployeCreditTransactions, opt => opt.Ignore())
                .ForMember(d => d.EmployeReglementsCredit, opt => opt.Ignore());
        }
    }
}
