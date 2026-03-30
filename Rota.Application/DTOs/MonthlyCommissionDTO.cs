namespace Rota.Application.DTOs
{
    public class MonthlyCommissionDTO
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty; 
        public decimal TotalTicketValue { get; set; }
        public decimal TotalLinkValue { get; set; }
        public decimal TotalInsuranceValue { get; set; }
        public decimal TotalSpecialVolumeValue { get; set; }
        public decimal TotalParcelValue { get; set; }
        public decimal CalculatedTicketCommission { get; set; }
        public decimal CalculatedLinkCommission { get; set; }
        public decimal CalculatedInsuranceCommission { get; set; }
        public decimal CalculatedSpecialVolumeCommission { get; set; }
        public decimal CalculatedParcelCommission { get; set; }

        public decimal CalculatedCommission { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalInvoiceDeductions { get; set; }
        public DateTime CalculationDate { get; set; }
    }
}