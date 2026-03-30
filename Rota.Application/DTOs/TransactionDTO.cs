using Rota.Domain.Enums;
using System;

namespace Rota.Application.DTOs
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public int TransactionIdApi { get; set; }
        
        public int AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;  
        
       
        public int? UserId { get; set; } 
        public string? UserName { get; set; } 
        
        public bool IsManual { get; set; } 
        

    }
}