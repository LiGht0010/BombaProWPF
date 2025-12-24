using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CategorieProfile : Profile
    {
        public CategorieProfile()
        {
            CreateMap<Categorie, CategorieDto>()
                .ReverseMap();
        }
    }
}
