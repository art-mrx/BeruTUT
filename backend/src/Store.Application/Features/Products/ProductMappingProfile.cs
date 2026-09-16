using AutoMapper;
using Store.Application.Features.Products.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Products;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Url)));

        CreateMap<ProductImage, ProductImageDto>();
    }
}
