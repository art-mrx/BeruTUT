using Store.Domain.Common;

namespace Store.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
