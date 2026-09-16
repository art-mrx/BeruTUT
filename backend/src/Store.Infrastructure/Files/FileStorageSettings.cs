namespace Store.Infrastructure.Files;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = null!;
    public string PublicBasePath { get; set; } = "/uploads/products";
}
