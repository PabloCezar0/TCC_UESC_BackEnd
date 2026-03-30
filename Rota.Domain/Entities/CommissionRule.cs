using Rota.Domain.Validation;
using System.Text.RegularExpressions;

namespace Rota.Domain.Entities
{
    public sealed class CommissionRule : EntityBase
    {
        public string Name { get; private set; } = string.Empty;
        public string TotalSalesFormula { get; private set; } = string.Empty;
        public string CommissionFormula { get; private set; } = string.Empty;

        
        private const string FormulaRegexPattern = @"^([\d\.\+\-\*\/\(\)\s]|VP|VL|VS|VV|VE|TP|TL|TS|TV|TE|DED|TOTAL)+$";

        public CommissionRule(string name, string totalSalesFormula, string commissionFormula)
        {
            Validate(name, totalSalesFormula, commissionFormula);
            Name = name;
            TotalSalesFormula = totalSalesFormula.ToUpper().Trim();
            CommissionFormula = commissionFormula.ToUpper().Trim();
        }

        public void Update(string name, string totalSalesFormula, string commissionFormula)
        {
            Validate(name, totalSalesFormula, commissionFormula);
            Name = name;
            TotalSalesFormula = totalSalesFormula.ToUpper().Trim();
            CommissionFormula = commissionFormula.ToUpper().Trim();
        }

        private void Validate(string name, string salesFormula, string commFormula)
        {
            DomainExceptionValidation.When(string.IsNullOrEmpty(name), "Nome é obrigatório");
            
            DomainExceptionValidation.When(string.IsNullOrEmpty(salesFormula), "Fórmula de vendas é obrigatória");
            DomainExceptionValidation.When(!Regex.IsMatch(salesFormula.ToUpper(), FormulaRegexPattern), 
                "Fórmula de Vendas inválida. Use apenas: VP, VL, VS, VV, VE, TP, TL, TS, TV, TE.");

            DomainExceptionValidation.When(string.IsNullOrEmpty(commFormula), "Fórmula de comissão é obrigatória");
            DomainExceptionValidation.When(!Regex.IsMatch(commFormula.ToUpper(), FormulaRegexPattern), 
                "Fórmula de Comissão inválida. Use apenas: TOTAL, DED e operadores.");
        }
    }
}