using Rota.Domain.Enums;
using System.IO;

public class InvoiceUploadDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public decimal Value { get; set; }
    public int AgencyId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public Stream Content { get; set; } = null!;
    public bool IsAnnual { get; set; }
}