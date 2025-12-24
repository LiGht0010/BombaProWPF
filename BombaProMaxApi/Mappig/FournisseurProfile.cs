using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class FournisseurProfile : Profile
    {
        public FournisseurProfile()
        {
            CreateMap<Fournisseur, FournisseurDto>()
                .ReverseMap();
        }
    }
}
