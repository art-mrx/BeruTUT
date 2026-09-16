using MediatR;
using Store.Application.Features.Products.Dtos;

namespace Store.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommand : IRequest<ProductImageDto>
{
    public Guid ProductId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public Stream Content { get; set; } = null!;
}
