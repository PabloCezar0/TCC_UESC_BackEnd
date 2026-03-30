using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using System; 
using System.Linq; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Rota.Domain.Constants;

namespace Rota.Application.Services
{
    public class CommissionCalculationService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IMonthlyCommissionRepository _monthlyCommissionRepository;
        private readonly IInvoiceRepository _invoiceRepository; 
        private readonly FormulaEvaluatorService _evaluator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommissionCalculationService(
            ITransactionRepository transactionRepository, 
            IAgencyRepository agencyRepository, 
            IMonthlyCommissionRepository monthlyCommissionRepository,
            IInvoiceRepository invoiceRepository,
            FormulaEvaluatorService evaluator,
            IHttpContextAccessor httpContextAccessor) 
        {
            _transactionRepository = transactionRepository;
            _agencyRepository = agencyRepository;
            _monthlyCommissionRepository = monthlyCommissionRepository;
            _invoiceRepository = invoiceRepository; 
            _evaluator = evaluator;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task CalculateAndSaveForPeriod(int year, int month, int? targetAgencyId = null)
        {

            

            IEnumerable<Agency> agencies;
          
            if (targetAgencyId.HasValue)
            {
                var agency = await _agencyRepository.GetByIdAsync(targetAgencyId.Value);
                
                agencies = agency != null ? new List<Agency> { agency } : new List<Agency>();
            }
            else
            {
                agencies = await _agencyRepository.GetAllAsync(); 
            }
            
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            foreach (var agency in agencies)
            {
                var transactions = await _transactionRepository.GetByAgencyAndPeriodAsync(agency.Id, startDate, endDate);
                var invoices = await _invoiceRepository.GetByAgencyAndPeriodAsync(agency.Id, year, month); 

                var valTicket = transactions.Where(t => t.Type == TransactionType.Passagem).Sum(t => t.Value);
                var valLink = transactions.Where(t => t.Type == TransactionType.Link).Sum(t => t.Value);
                var valIns = transactions.Where(t => t.Type == TransactionType.Seguro).Sum(t => t.Value);
                var valVol = transactions.Where(t => t.Type == TransactionType.Volume).Sum(t => t.Value);
                var valPar = 0m; 
                
                var valDeductions = invoices.Sum(i => i.Value);

                MonthlyCommission commissionEntity;

                if (agency.CommissionRuleId.HasValue && agency.CommissionRule != null)
                {
                    var variables = new Dictionary<string, decimal>
                    {
                        { CalculationConstants.ValTicket, valTicket },
                        { CalculationConstants.ValLink, valLink },
                        { CalculationConstants.ValInsurance, valIns },
                        { CalculationConstants.ValSpecialVolume, valVol },
                        { CalculationConstants.ValParcel, valPar },
                        
                        { CalculationConstants.FeeTicket, (decimal)agency.FeeTicket / 100m },
                        { CalculationConstants.FeeLink, (decimal)agency.FeeInternet / 100m },
                        { CalculationConstants.FeeInsurance, (decimal)agency.FeeInsurance / 100m },
                        { CalculationConstants.FeeSpecialVolume, (decimal)agency.FeeSpecialVolume / 100m },
                        { CalculationConstants.FeeParcel, (decimal)agency.FeePackage / 100m },
                        
                        { CalculationConstants.Deduction, valDeductions }
                    };

                    var totalSales = _evaluator.Evaluate(agency.CommissionRule.TotalSalesFormula, variables);
                    variables[CalculationConstants.Total] = totalSales;
                    
                    var finalValue = _evaluator.Evaluate(agency.CommissionRule.CommissionFormula, variables);

                    commissionEntity = MonthlyCommission.CreateCustom(
                        year, month, agency.Id,
                        valTicket, valLink, valIns, valVol, valPar,
                        finalValue, valDeductions
                    );
                }
                else
                {
                    var cTicket = valTicket * (decimal)(agency.FeeTicket / 100);
                    var cLink   = valLink   * (decimal)(agency.FeeInternet / 100);
                    var cIns    = valIns    * (decimal)(agency.FeeInsurance / 100);
                    var cVol    = valVol    * (decimal)(agency.FeeSpecialVolume / 100);
                    var cPar    = valPar    * (decimal)(agency.FeePackage / 100);

                    commissionEntity = MonthlyCommission.CreateStandard(
                        year, month, agency.Id,
                        valTicket, valLink, valIns, valVol, valPar,
                        cTicket, cLink, cIns, cVol, cPar,
                        valDeductions
                    );
                }

                await _monthlyCommissionRepository.AddOrUpdateAsync(commissionEntity);
            }
        }
    }
}