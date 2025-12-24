using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class EmployeProfile : Profile
    {
        public EmployeProfile()
        {
            CreateMap<Employe, EmployeDto>()
                .ReverseMap();
        }
    }
}
