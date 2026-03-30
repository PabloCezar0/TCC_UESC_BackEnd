using Rota.Domain.Entities;

namespace Rota.Domain.Entities;

public sealed class InvoiceFile : EntityBase          
{
    public string   Name        { get; private set; } = null!;
    public string   ContentType { get; private set; } = null!;
    public DateTime UploadedAt  { get; private set; }
    public string   Url         { get; private set; } = null!;


    public int      InvoiceId   { get; private set; }
    public Invoice  Invoice     { get; private set; } = null!;

    private InvoiceFile() { }

    public InvoiceFile(string name, string contentType,
                       DateTime uploadedAt, string url, int invoiceId)
    {
        Name        = name;
        ContentType = contentType;
        UploadedAt  = uploadedAt;
        Url         = url;
        InvoiceId   = invoiceId;
    }
}
