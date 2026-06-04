using AutoMapper;
using FourniProApi.DTOs;
using FourniProApi.Models;

namespace FourniProApi.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Permissions,
                opt => opt.MapFrom(src =>
                    src.UserPermissions
                        .Select(up => up.Permission.Name)
                        .ToList()));
    }
}
