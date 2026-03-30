using Rota.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rota.Application.DTOs
{
    public class InvoiceUpdateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Value { get; set; }
        [Required]
        public DateTime IssueDate { get; set; }
        [Required]
        public bool IsAnnual { get; set; }
    }
}