using Microsoft.Extensions.Options;
using Store.Application.Common.Interfaces;

namespace Store.Infrastructure.Files;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_settings.RootPath);

        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_settings.RootPath, storedFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{_settings.PublicBasePath}/{storedFileName}";
    }
}
