using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class EmployeCreditTransactionProfile : Profile
    {
        public EmployeCreditTransactionProfile()
        {
            CreateMap<EmployeCreditTransaction, EmployeCreditTransactionDto>()
                .ForMember(d => d.EmployeNom,
                    opt => opt.MapFrom(s => s.Employe != null ? s.Employe.Nom : null))
                .ReverseMap();
        }
    }
}
