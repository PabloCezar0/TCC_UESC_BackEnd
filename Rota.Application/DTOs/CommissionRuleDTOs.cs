using System.Collections.Generic;

namespace Rota.Application.DTOs
{
  
    public class CommissionRuleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TotalSalesFormula { get; set; } = string.Empty;
        public string CommissionFormula { get; set; } = string.Empty;
    }

    public class RuleCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public string TotalSalesFormula { get; set; } = string.Empty;
        public string CommissionFormula { get; set; } = string.Empty;
    }

    public class RulePreviewRequestDTO : RuleCreateDTO
    {
   
        public string Name { get; set; } = string.Empty;
        public string TotalSalesFormula { get; set; } = string.Empty;
        public string CommissionFormula { get; set; } = string.Empty;

        
        public decimal? TestVP { get; set; }
        public decimal? TestVL { get; set; }
        public decimal? TestVS { get; set; } 
        public decimal? TestVV { get; set; }
        public decimal? TestVE { get; set; } 
        
        
        public decimal? TestTP { get; set; } 
        public decimal? TestTL { get; set; } 
        public decimal? TestTS { get; set; }
        public decimal? TestTV { get; set; } 
        public decimal? TestTE { get; set; } 

       
        public decimal? TestDED { get; set; } 


    }

public class RulePreviewResponseDTO
{
    public bool Success { get; set; }
    public Dictionary<string, decimal> UsedVariables { get; set; } = new();
    
    
    public decimal CalculatedTotalSales { get; set; }     
    public decimal CalculatedGrossCommission { get; set; } 
    
    
    public decimal StandardDeduction { get; set; }        
    public decimal FinalPaymentValue { get; set; }        
    
    public string? ErrorMessage { get; set; }
}
}