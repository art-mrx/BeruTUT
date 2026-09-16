using AutoMapper;
using Store.Application.Features.Auth.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Auth;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
