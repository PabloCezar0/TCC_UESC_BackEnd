using Rota.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Rota.Application.DTOs
{
    public class TransactionRegisterManualDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
        public decimal Value { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int AgencyId { get; set; }

        [Required]
        public ManualTransactionType Type { get; set; }
    }
}