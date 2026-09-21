using AutoMapper;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Orders;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Price * src.Quantity));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Order, AdminOrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AllowedNextStatuses, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}
