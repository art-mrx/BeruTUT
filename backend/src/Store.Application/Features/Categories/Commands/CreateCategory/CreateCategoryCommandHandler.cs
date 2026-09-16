using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Categories.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slugExists = await _context.Categories.AnyAsync(c => c.Slug == request.Slug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictException($"A category with slug \"{request.Slug}\" already exists.");
        }

        if (request.ParentCategoryId is not null)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentCategoryId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException(nameof(Category), request.ParentCategoryId);
            }
        }

        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            ParentCategoryId = request.ParentCategoryId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}
