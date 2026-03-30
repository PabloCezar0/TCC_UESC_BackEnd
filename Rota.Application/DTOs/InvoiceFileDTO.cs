public class InvoiceFileDTO
{
    public string Name { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public string Url { get; set; } = null!;
    public int InvoiceId { get; set; }
}