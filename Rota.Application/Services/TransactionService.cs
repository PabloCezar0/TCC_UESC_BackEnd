using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Rota.Application.DTOs;
using Rota.Application.Interface;
using Rota.Domain.Common;
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Rota.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly CommissionCalculationService _commissionCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAgencyRepository _agencyRepository;

        public TransactionService(
            ITransactionRepository transactionRepository, 
            IMapper mapper, 
            CommissionCalculationService commissionCalculationService, 
            IHttpContextAccessor httpContextAccessor,
            IAgencyRepository agencyRepository)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _commissionCalculationService = commissionCalculationService;
            _httpContextAccessor = httpContextAccessor;
            _agencyRepository = agencyRepository;
        }

    
        private bool IsAdminOrFinance()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim == UserRole.Administrador.ToString() || 
                   roleClaim == UserRole.Financeiro.ToString();
        }

        private int? GetUserAgencyId()
        {
            var agencyClaim = _httpContextAccessor.HttpContext?.User.FindFirst("AgencyId")?.Value;
            return int.TryParse(agencyClaim, out var id) ? id : null;
        }
 

        public async Task AddAsync(TransactionRegisterDTO transactionDTO)
        {
          
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency == null)
                    throw new UnauthorizedAccessException("Usuário sem agência vinculada.");

                
                if (transactionDTO.AgencyId != userAgency.Value)
                {
                    throw new UnauthorizedAccessException("Você não pode criar transações para outra agência.");
                }
            }

            var transaction = _mapper.Map<Transaction>(transactionDTO);
            await _transactionRepository.AddAsync(transaction);
        }
        public async Task<IEnumerable<TransactionDTO>> GetAllAsync()
        {
            
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                
                if (userAgency == null) 
                    throw new UnauthorizedAccessException("Usuário sem agência vinculada.");

                
                var agencyTransactions = await _transactionRepository.GetByAgencyAsync(userAgency.Value);
                return _mapper.Map<IEnumerable<TransactionDTO>>(agencyTransactions);
            }

            
            var allTransactions = await _transactionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TransactionDTO>>(allTransactions);
        }

        public async Task<TransactionDTO> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Transação não encontrada.");
            
            
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency != transaction.AgencyId)
                    throw new UnauthorizedAccessException("Acesso negado.");
            }

            return _mapper.Map<TransactionDTO>(transaction);
        }

        public async Task<PaginatedResult<TransactionDTO>> GetPaginatedTransactionsAsync(
            int pageNumber, int pageSize, string? type, int? agencyId, 
            decimal? minValue, decimal? maxValue, DateTime? startDate, DateTime? endDate)
        {
            
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency == null) throw new UnauthorizedAccessException("Usuário sem agência.");
                agencyId = userAgency.Value;
            }

            var paginatedResult = await _transactionRepository.GetPaginatedAsync(
                pageNumber, pageSize, type, agencyId, minValue, maxValue, startDate, endDate);

            var transactionsDTO = _mapper.Map<List<TransactionDTO>>(paginatedResult.Items);


            var missingAgencyIds = transactionsDTO
                .Where(t => string.IsNullOrEmpty(t.AgencyName))
                .Select(t => t.AgencyId)
                .Distinct()
                .ToList();

            if (missingAgencyIds.Any())
            {
               
                foreach (var id in missingAgencyIds)
                {

                    var agency = await _agencyRepository.GetByIdWithDeletedAsync(id); 
                    
                    if (agency != null)
                    {
                       
                        var toUpdate = transactionsDTO.Where(t => t.AgencyId == id);
                        foreach (var dto in toUpdate)
                        {
                            dto.AgencyName = agency.CorporateName;
                        }
                    }
                }
            }
           

            return new PaginatedResult<TransactionDTO>
            {
                TotalItems = paginatedResult.TotalItems,
                PageNumber = paginatedResult.PageNumber,
                PageSize = paginatedResult.PageSize,
                Items = transactionsDTO
            };
        }

        public async Task RemoveAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Transação não encontrada");

          
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency != transaction.AgencyId)
                    throw new UnauthorizedAccessException("Acesso negado ao excluir.");
            }

            var yearToRecalculate = transaction.Date.Year;
            var monthToRecalculate = transaction.Date.Month;
            var agencyIdToRecalculate = transaction.AgencyId;
            
            await _transactionRepository.RemoveAsync(id);
            await _commissionCalculationService.CalculateAndSaveForPeriod(yearToRecalculate, monthToRecalculate, agencyIdToRecalculate);
        }

        public async Task UpdateAsync(int id, TransactionRegisterDTO transactionDTO)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Transação não encontrada");

           
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency != transaction.AgencyId)
                    throw new UnauthorizedAccessException("Acesso negado ao editar.");
            }

            transaction.Update(
                transactionDTO.Type,
                transactionDTO.Value,
                transactionDTO.Date,
                transactionDTO.TransactionIdApi,
                transactionDTO.AgencyId);

            await _transactionRepository.UpdateAsync(transaction);
        }

        public async Task<TransactionDTO> CreateManualAsync(TransactionRegisterManualDTO dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuário não identificado.");
            }

          
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
           
                if (userAgency != dto.AgencyId)
                {
                    throw new UnauthorizedAccessException("Você só pode criar transações para sua própria agência.");
                }
            }
            var typeConverted = (TransactionType)dto.Type;

            var newTransaction = Transaction.CreateManual(typeConverted, dto.Value, dto.Date, dto.AgencyId, userId);

            await _transactionRepository.AddAsync(newTransaction);
                
            await _commissionCalculationService.CalculateAndSaveForPeriod(newTransaction.Date.Year, newTransaction.Date.Month, newTransaction.AgencyId);
                
            return _mapper.Map<TransactionDTO>(newTransaction);
        }
    }
}