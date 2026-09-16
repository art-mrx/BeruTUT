using AutoMapper;
using Store.Application.Features.Categories.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Categories;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>();
    }
}
