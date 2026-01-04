using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CategorieProfile : Profile
    {
        public CategorieProfile()
        {
            // Entity to DTO mapping
            CreateMap<Categorie, CategorieDto>();

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<CategorieDto, Categorie>()
                .ForMember(d => d.Produits, opt => opt.Ignore());
        }
    }
}
