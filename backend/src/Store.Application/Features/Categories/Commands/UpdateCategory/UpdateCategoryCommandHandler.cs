using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Categories.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        var slugTaken = await _context.Categories
            .AnyAsync(c => c.Slug == request.Slug && c.Id != request.Id, cancellationToken);
        if (slugTaken)
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

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.ParentCategoryId = request.ParentCategoryId;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}
