using AutoMapper;
using Store.Application.Features.Users.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Users;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, AdminUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.OrdersCount, opt => opt.MapFrom(src => src.Orders.Count));
    }
}
