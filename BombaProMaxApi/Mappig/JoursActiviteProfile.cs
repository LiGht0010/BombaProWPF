using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class JoursActiviteProfile : Profile
    {
        public JoursActiviteProfile()
        {
            CreateMap<JoursActivite, JoursActiviteDto>()
                .ReverseMap();
        }
    }
}
