using Rota.Domain.Enums;
using System;
using System.Collections.Generic;

public class InvoiceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Value { get; set; }
    public DateTime IssueDate { get; set; }
    public int ReferenceYear { get; set; }
    public int ReferenceMonth { get; set; }
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty; 
    public IEnumerable<InvoiceFileDTO> Files { get; set; } = new List<InvoiceFileDTO>();
    public bool IsAnnual { get; set; }
}