using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class DepenseProfile : Profile
    {
        public DepenseProfile()
        {
            CreateMap<Depense, DepenseDto>()
                .ReverseMap();
        }
    }
}
