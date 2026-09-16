using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Products.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        if (product.CategoryId != request.CategoryId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);
            product.Category = category;
            product.CategoryId = request.CategoryId;
        }

        if (product.Slug != request.Slug)
        {
            var slugTaken = await _context.Products
                .AnyAsync(p => p.Slug == request.Slug && p.Id != request.Id, cancellationToken);
            if (slugTaken)
            {
                throw new ConflictException($"A product with slug \"{request.Slug}\" already exists.");
            }
        }

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }
}
