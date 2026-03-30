using Rota.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Rota.Application.Services
{
    public class TransactionInsertService
    {
        private readonly TransactionImportService _importService;
        private readonly ITransactionRepository _transactionRepository;

        private readonly CommissionCalculationService _commissionCalculationService; 
        private readonly ILogger<TransactionInsertService> _logger;

        public TransactionInsertService(TransactionImportService importService, ITransactionRepository transactionRepository, CommissionCalculationService commissionCalculationService,
            ILogger<TransactionInsertService> logger) 
        {
            _importService = importService;
            _transactionRepository = transactionRepository;
            _commissionCalculationService = commissionCalculationService; 
            _logger = logger;
        }



        public async Task<int> ImportAndSaveMonthlyCycleAsync()
        {
            var today = DateTime.UtcNow;

            var endDate = new DateTime(today.Year, today.Month, 25);
            var startDate = endDate.AddMonths(-1).AddDays(1);

            return await ImportAndSavePeriodAsync(startDate, endDate);
        }


        public async Task<int> ImportAndSavePeriodAsync(DateTime startDate, DateTime endDate)
        {
            var transactionsToImport = await _importService.ImportFromPeriodAsync(startDate, endDate);
            
            if (transactionsToImport == null || !transactionsToImport.Any())
            {
                return 0;
            }

            var affectedPeriods = transactionsToImport
                .Select(t => new { t.Date.Year, t.Date.Month })
                .Distinct()
                .ToList();

            int savedCount = 0;
            
            foreach (var transaction in transactionsToImport)
            {
                bool transactionExists = await _transactionRepository.ExistsAsync(
                    transaction.TransactionIdApi,
                    transaction.Type,
                    transaction.AgencyId);

                if (!transactionExists)
                {
                    await _transactionRepository.AddAsync(transaction);
                    savedCount++;
                }
            }
            _logger.LogInformation("{SavedCount} novas transações salvas no banco de dados.", savedCount);
            
            if (savedCount > 0)
            {
                _logger.LogInformation("Disparando recálculo de comissão para os períodos afetados.");
                foreach (var period in affectedPeriods)
                {
                    _logger.LogInformation("Recalculando comissões para {Month}/{Year}...", period.Month, period.Year);
                    await _commissionCalculationService.CalculateAndSaveForPeriod(period.Year, period.Month);
                }
                _logger.LogInformation("Recálculo de comissão concluído.");
            }

            return savedCount;
        }
    }
}