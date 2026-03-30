using System.ComponentModel.DataAnnotations;

namespace Rota.Application.DTOs
{
    public class AgencyRegisterDTO
    {
        public string ExternalId { get; set; } = string.Empty;

        [Required]
        public string CorporateName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}", ErrorMessage = "CNPJ inválido.")]
        public string CNPJ { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(2, ErrorMessage = "Estado deve ter 2 caracteres.")]
        public string State { get; set; } = string.Empty;

        [Required]
        public string AddressNumber { get; set; } = string.Empty;
        public string AddressComment { get; set; } = string.Empty;

        [Required]
        public string PhoneNumberOne { get; set; } = string.Empty;
        public string PhoneNumberTwo { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
