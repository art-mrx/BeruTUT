using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Domain.Entities;

namespace Store.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
        if (hasProducts)
        {
            throw new ConflictException("Cannot delete a category that still has products assigned to it.");
        }

        var hasSubCategories = await _context.Categories.AnyAsync(c => c.ParentCategoryId == request.Id, cancellationToken);
        if (hasSubCategories)
        {
            throw new ConflictException("Cannot delete a category that has subcategories.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
