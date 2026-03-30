namespace Rota.Domain.Common;

public class PaginatedResult<T>
{
    public int PageNumber  { get; set; }
    public int PageSize    { get; set; }
    public int TotalItems  { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int TotalPages =>
        PageSize == 0 ? 0
                      : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
