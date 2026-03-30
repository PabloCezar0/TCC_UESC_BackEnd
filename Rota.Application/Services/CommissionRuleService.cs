using Rota.Application.DTOs;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Rota.Application.Services
{
    public class CommissionRuleService
    {
        private readonly FormulaEvaluatorService _evaluator;
        private readonly ICommissionRuleRepository _repo;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommissionRuleService(FormulaEvaluatorService evaluator, ICommissionRuleRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _evaluator = evaluator;
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        private void ValidateAdmin()
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Administrador" && role != "Financeiro")
            {
                throw new UnauthorizedAccessException("Apenas administradores podem gerenciar regras de comissão.");
            }
        }

        public async Task<IEnumerable<CommissionRuleDTO>> GetAllAsync()
        {
            var rules = await _repo.GetAllAsync();
            
            return rules.Select(r => new CommissionRuleDTO
            {
                Id = r.Id,
                Name = r.Name,
                TotalSalesFormula = r.TotalSalesFormula,
                CommissionFormula = r.CommissionFormula
            });
        }

        public async Task CreateAsync(RuleCreateDTO dto)
        {
            ValidateAdmin();
            var preview = Preview(new RulePreviewRequestDTO 
            { 
                Name = dto.Name,
                TotalSalesFormula = dto.TotalSalesFormula, 
                CommissionFormula = dto.CommissionFormula 
            });

            if (!preview.Success) 
                throw new InvalidOperationException($"Fórmula inválida: {preview.ErrorMessage}");

            var entity = new CommissionRule(dto.Name, dto.TotalSalesFormula, dto.CommissionFormula);
            await _repo.AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            ValidateAdmin();

            await _repo.DeleteAsync(id);
        }


        public RulePreviewResponseDTO Preview(RulePreviewRequestDTO dto)
            {
                
                var variables = new Dictionary<string, decimal>
                {
                    
                    { "VP", dto.TestVP ?? 0m },
                    { "VL", dto.TestVL ?? 0m },
                    { "VS", dto.TestVS ?? 0m },
                    { "VV", dto.TestVV ?? 0m },
                    { "VE", dto.TestVE ?? 0m },
                    

                    { "TP", dto.TestTP ?? 0m },
                    { "TL", dto.TestTL ?? 0m },
                    { "TS", dto.TestTS ?? 0m },
                    { "TV", dto.TestTV ?? 0m },
                    { "TE", dto.TestTE ?? 0m },
                    
                   
                    { "DED", dto.TestDED ?? 0m }
                };

                try
                {
                    var totalSales = _evaluator.Evaluate(dto.TotalSalesFormula, variables);
                    
                    
                    if (variables.ContainsKey("TOTAL")) variables["TOTAL"] = totalSales;
                    else variables.Add("TOTAL", totalSales);

                    var grossCommission = _evaluator.Evaluate(dto.CommissionFormula, variables);

                   
                    var finalPayment = grossCommission - variables["DED"];

                    return new RulePreviewResponseDTO
                    {
                        Success = true,
                        UsedVariables = variables,
                        CalculatedTotalSales = totalSales,
                        CalculatedGrossCommission = grossCommission,
                        StandardDeduction = variables["DED"],
                        FinalPaymentValue = finalPayment
                    };
                }
                catch (Exception ex)
                {
                    return new RulePreviewResponseDTO { Success = false, ErrorMessage = ex.Message };
                }
        }
    }
}