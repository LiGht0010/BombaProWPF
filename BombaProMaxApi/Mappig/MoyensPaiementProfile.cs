using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class MoyensPaiementProfile : Profile
    {
        public MoyensPaiementProfile()
        {
            CreateMap<MoyensPaiement, MoyensPaiementDto>()
                .ReverseMap();
        }
    }
}
