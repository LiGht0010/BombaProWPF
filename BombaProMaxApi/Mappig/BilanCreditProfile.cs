using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class BilanCreditProfile : Profile
    {
        public BilanCreditProfile()
        {
            CreateMap<BilanCredit, BilanCreditDto>()
                .ForMember(d => d.ClientNom,
                    opt => opt.MapFrom(s => s.Client != null ? s.Client.Nom : null))
                .ReverseMap();
        }
    }
}
