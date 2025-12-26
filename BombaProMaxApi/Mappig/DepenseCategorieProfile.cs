using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class DepenseCategorieProfile : Profile
{
    public DepenseCategorieProfile()
    {
        CreateMap<DepenseCategorie, DepenseCategorieDto>()
            .ReverseMap();
    }
}
