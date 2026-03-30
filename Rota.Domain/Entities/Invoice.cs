using System.ComponentModel.DataAnnotations.Schema;
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Validation;

public class Invoice : EntityBase
{

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; private set; }
    public DateTime IssueDate { get; private set; }
    public int ReferenceYear { get; private set; }
    public int ReferenceMonth { get; private set; }
    public int AgencyId { get; private set; }
    public Agency Agency { get; private set; } = null!;
    public ICollection<InvoiceFile> Files { get; private set; } = new List<InvoiceFile>();
    public bool IsAnnual { get; private set; }


    public int UserId { get; private set; } 
    public User User { get; private set; } = null!;

    protected Invoice() { }

    public Invoice(string name, string description, decimal value, DateTime issueDate, int referenceYear, int referenceMonth, int agencyId, bool isAnnual, int userId)
    {
        ValidateDomain(name, value, agencyId);
        Name = name;
        Description = description;
        Value = value;
        IssueDate = issueDate;
        ReferenceYear = referenceYear;
        ReferenceMonth = referenceMonth;
        AgencyId = agencyId;
        IsAnnual = isAnnual;
        UserId = userId; 
    }
    
    public void Update(string name, string description, decimal value, DateTime issueDate, bool isAnnual)
    {
        ValidateDomain(name, value, this.AgencyId);
        Name = name;
        Description = description;
        Value = value;
        IssueDate = issueDate;
        ReferenceYear = issueDate.Year;   
        ReferenceMonth = issueDate.Month; 
        IsAnnual = isAnnual;
    }

    private void ValidateDomain(string name, decimal value, int agencyId)
    {
        DomainExceptionValidation.When(string.IsNullOrEmpty(name), "O nome da nota fiscal é obrigatório.");
        DomainExceptionValidation.When(value <= 0, "O valor da nota fiscal deve ser positivo.");
        DomainExceptionValidation.When(agencyId <= 0, "O ID da agência é inválido.");
    }
}