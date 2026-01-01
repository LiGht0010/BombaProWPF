using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User -> UserDto (include all fields)
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.createdBy))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.updatedBy));

        // UserDto -> User (include all fields, but ignore null Password to prevent overwriting during updates)
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.createdBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.updatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.Password, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Password)));
    }
}
