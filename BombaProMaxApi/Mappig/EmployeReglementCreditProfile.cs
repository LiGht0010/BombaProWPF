using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class EmployeReglementCreditProfile : Profile
    {
        public EmployeReglementCreditProfile()
        {
            CreateMap<EmployeReglementCredit, EmployeReglementCreditDto>()
                .ForMember(d => d.EmployeNom,
                    opt => opt.MapFrom(s => s.Employe != null ? s.Employe.Nom : null))
                .ForMember(d => d.ModePaiementNom,
                    opt => opt.MapFrom(s => s.ModePaiement != null ? s.ModePaiement.Nom : null))
                .ReverseMap();
        }
    }
}
