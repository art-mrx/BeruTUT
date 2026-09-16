using AutoMapper;
using Store.Application.Features.Carts.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Carts;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Url)
                .FirstOrDefault()))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Product.Price * src.Quantity));

        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.Product.Price * i.Quantity)));
    }
}
