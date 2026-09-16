using FluentValidation;

namespace Store.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}.");
    }
}
