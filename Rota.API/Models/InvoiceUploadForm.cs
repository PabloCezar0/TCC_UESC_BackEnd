using Microsoft.AspNetCore.Http;
using Rota.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rota.API.Models
{
    public class InvoiceUploadForm
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime IssueDate { get; set; }
        [Required]
        public string Value { get; set; } = string.Empty;       
        [Required]
        public int AgencyId { get; set; }
        [Required]
        public bool IsAnnual { get; set; } 
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}