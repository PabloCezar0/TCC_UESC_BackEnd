using System.Data;
using System.Globalization;

namespace Rota.Application.Services
{
    public class FormulaEvaluatorService
    {
        public decimal Evaluate(string formula, Dictionary<string, decimal> variables)
        {
            try
            {
                string expression = formula.ToUpper();
                var orderedVariables = variables.OrderByDescending(v => v.Key.Length);

                foreach (var variable in variables)
                {
                   
                    string valStr = variable.Value.ToString("F2", CultureInfo.InvariantCulture);
                    expression = expression.Replace(variable.Key, valStr);
                }

                var result = new DataTable().Compute(expression, null);
                
                if (result == null) return 0m;
                
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao calcular fórmula: {ex.Message}");
            }
        }
    }
}