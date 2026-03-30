using Rota.Domain.Entities;
using System;

namespace Rota.Domain.Entities
{
    public sealed class MonthlyCommission : EntityBase
    {
        
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int AgencyId { get; private set; }
        public Agency Agency { get; private set; } = null!;
        

        public decimal TotalTicketValue { get; private set; }
        public decimal TotalLinkValue { get; private set; }
        public decimal TotalInsuranceValue { get; private set; }
        public decimal TotalSpecialVolumeValue { get; private set; }
        public decimal TotalParcelValue { get; private set; } 

        public decimal CalculatedTicketCommission { get; private set; }
        public decimal CalculatedLinkCommission { get; private set; }
        public decimal CalculatedInsuranceCommission { get; private set; }
        public decimal CalculatedSpecialVolumeCommission { get; private set; }
        public decimal CalculatedParcelCommission { get; private set; } 
        
        public decimal CalculatedCommission { get; private set; } 
        public decimal TotalCommission { get; private set; } 
        public decimal TotalInvoiceDeductions { get; private set; }
        
        public DateTime CalculationDate { get; private set; }

        private MonthlyCommission() {}

        private MonthlyCommission(
            int year, int month, int agencyId, 
            decimal tTicket, decimal tLink, decimal tIns, decimal tVol, decimal tPar,
            decimal cTicket, decimal cLink, decimal cIns, decimal cVol, decimal cPar, 
            decimal calcTotal, decimal deductions)
        {
            Year = year;
            Month = month;
            AgencyId = agencyId;
            
            TotalTicketValue = tTicket;
            TotalLinkValue = tLink;
            TotalInsuranceValue = tIns;
            TotalSpecialVolumeValue = tVol;
            TotalParcelValue = tPar;
            
            CalculatedTicketCommission = cTicket;
            CalculatedLinkCommission = cLink;
            CalculatedInsuranceCommission = cIns;
            CalculatedSpecialVolumeCommission = cVol;
            CalculatedParcelCommission = cPar;
            
            CalculatedCommission = calcTotal;
            TotalInvoiceDeductions = deductions;
            TotalCommission = CalculatedCommission - TotalInvoiceDeductions;
            
            CalculationDate = DateTime.UtcNow;
        }

        public static MonthlyCommission CreateStandard(
            int year, int month, int agencyId,
            decimal tTicket, decimal tLink, decimal tIns, decimal tVol, decimal tPar,
            decimal cTicket, decimal cLink, decimal cIns, decimal cVol, decimal cPar,
            decimal deductions)
        {
            var totalBruto = cTicket + cLink + cIns + cVol + cPar;
            return new MonthlyCommission(year, month, agencyId, 
                tTicket, tLink, tIns, tVol, tPar, 
                cTicket, cLink, cIns, cVol, cPar, 
                totalBruto, deductions);
        }

        public static MonthlyCommission CreateCustom(
            int year, int month, int agencyId,
            decimal tTicket, decimal tLink, decimal tIns, decimal tVol, decimal tPar,
            decimal customTotalCalculated,
            decimal deductions)
        {
            return new MonthlyCommission(year, month, agencyId, 
                tTicket, tLink, tIns, tVol, tPar, 
                0, 0, 0, 0, 0, 
                customTotalCalculated, deductions);
        }
        
        public void UpdateValues(decimal totalCommission, decimal deductions)
        {
             CalculatedCommission = totalCommission;
             TotalInvoiceDeductions = deductions;
             TotalCommission = CalculatedCommission - TotalInvoiceDeductions;
             UpdatedAt = DateTime.UtcNow;
        }
        
        
        public void SetTotalCommission(decimal total)
        {
            CalculatedCommission = total; 
            TotalCommission = CalculatedCommission - TotalInvoiceDeductions; 
        }
    }
}