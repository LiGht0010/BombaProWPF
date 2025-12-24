using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class EmployeBilanCreditProfile : Profile
    {
        public EmployeBilanCreditProfile()
        {
            CreateMap<EmployeBilanCredit, EmployeBilanCreditDto>()
                .ForMember(d => d.EmployeNom,
                    opt => opt.MapFrom(s => s.Employe != null ? s.Employe.Nom : null))
                .ReverseMap();
        }
    }
}
