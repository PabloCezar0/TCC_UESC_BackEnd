using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rota.Domain.Entities;
using Rota.Domain.Enums;

namespace Rota.Application.DTOs
{
    public class TransactionRegisterDTO
    {
        public TransactionType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public int TransactionIdApi { get; set; }
        public int AgencyId { get; set; }
        
    }
}