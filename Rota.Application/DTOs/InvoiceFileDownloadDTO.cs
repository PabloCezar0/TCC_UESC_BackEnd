public sealed class InvoiceFileDownloadDTO
{
    public Stream Content     { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public string FileName    { get; init; } = null!;
}
