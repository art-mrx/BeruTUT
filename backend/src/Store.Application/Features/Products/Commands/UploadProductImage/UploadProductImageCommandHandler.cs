using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Products.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ProductImageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UploadProductImageCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService, IMapper mapper)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<ProductImageDto> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var url = await _fileStorageService.SaveAsync(request.Content, request.FileName, request.ContentType, cancellationToken);

        var nextSortOrder = await _context.ProductImages
            .Where(i => i.ProductId == product.Id)
            .CountAsync(cancellationToken);

        var image = new ProductImage
        {
            ProductId = product.Id,
            Url = url,
            SortOrder = nextSortOrder
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductImageDto>(image);
    }
}
