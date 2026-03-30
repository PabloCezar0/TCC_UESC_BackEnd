using System.Text.Json;
using Rota.Application.DTOs;
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces; 
using Microsoft.Extensions.Logging;

namespace Rota.Application.Services
{
    public class TransactionImportService 
    {
        private readonly HttpClient _httpClient;
        private readonly IAgencyRepository _agencyRepository; 
        private readonly ILogger<TransactionImportService> _logger;
        private const string Url = "http://localhost:3000/transactions";

       public TransactionImportService(
            HttpClient httpClient, 
            IAgencyRepository agencyRepository, 
            ILogger<TransactionImportService> logger)
        {
            _httpClient = httpClient;
            _agencyRepository = agencyRepository;
            _logger = logger;
        }
        
        public async Task<List<Transaction>> ImportFromPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var response = await _httpClient.GetAsync(Url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var transactionsDto = JsonSerializer.Deserialize<List<TransactionJsonServerDTO>>(json);

            if (transactionsDto == null || !transactionsDto.Any())
                return new List<Transaction>();

            var transactions = new List<Transaction>();
            var filteredTransactions = transactionsDto.Where(t =>
                t.SaleDatetimeH >= startDate && t.SaleDatetimeH <= endDate &&
                t.CancellationFlag == 0);

            foreach (var dto in filteredTransactions)
            {
                
                var agency = await _agencyRepository.FindByExternalIdAsync(dto.AgencyId.ToString());

                if (agency == null)
                {
                    _logger.LogWarning("Agência com ExternalId {ExternalId} não foi encontrada no banco. A transação {TransactionId} da API foi ignorada.", dto.AgencyId, dto.TransactionId);
                    continue;
                }
                
                if (dto.CategoryId == 1)
                {
                    
                    if (dto.PaidPrice > 0)
                    {
                        var transactionType = dto.SaleTypeId == 12 ? TransactionType.Link : TransactionType.Passagem;

                        
                        transactions.Add(Transaction.CreateAutomatic(
                            transactionType, 
                            dto.PaidPrice, 
                            dto.SaleDatetimeH, 
                            dto.TransactionId, 
                            agency.Id)); 
                    }

                    
                    if (dto.InsuranceAmount > 0)
                    {
                        
                        transactions.Add(Transaction.CreateAutomatic(
                            TransactionType.Seguro, 
                            dto.InsuranceAmount, 
                            dto.SaleDatetimeH, 
                            dto.TransactionId, 
                            agency.Id));
                    }

                   
                    if (dto.ExtraChargePrice > 0)
                    {
                        
                        transactions.Add(Transaction.CreateAutomatic(
                            TransactionType.Volume, 
                            dto.ExtraChargePrice, 
                            dto.SaleDatetimeH, 
                            dto.TransactionId, 
                            agency.Id));
                    }
                }
            }

            return transactions;
        }
    }
}