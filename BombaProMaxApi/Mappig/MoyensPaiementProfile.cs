using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class MoyensPaiementProfile : Profile
    {
        public MoyensPaiementProfile()
        {
            // Entity to DTO mapping
            CreateMap<MoyensPaiement, MoyensPaiementDto>();

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<MoyensPaiementDto, MoyensPaiement>()
                .ForMember(d => d.Factures, opt => opt.Ignore())
                .ForMember(d => d.ReglementsCredit, opt => opt.Ignore())
                .ForMember(d => d.EmployeReglementsCredit, opt => opt.Ignore());
        }
    }
}
