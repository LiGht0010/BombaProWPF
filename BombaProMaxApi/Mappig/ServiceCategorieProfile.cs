using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class ServiceCategorieProfile : Profile
{
    public ServiceCategorieProfile()
    {
        CreateMap<ServiceCategorie, ServiceCategorieDto>()
            .ReverseMap();
    }
}
