using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class IndicateursFinancierProfile : Profile
    {
        public IndicateursFinancierProfile()
        {
            CreateMap<IndicateursFinancier, IndicateursFinancierDto>()
                .ReverseMap();
        }
    }
}
