using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rota.Application.DTOs;
using Rota.Domain.Common;

namespace Rota.Application.Interface
{
    public interface ITransactionService
    {
        Task AddAsync(TransactionRegisterDTO transactionDTO);
        Task<TransactionDTO> CreateManualAsync(TransactionRegisterManualDTO dto);
        Task UpdateAsync(int id, TransactionRegisterDTO transactionDTO);
        Task RemoveAsync(int id);
        Task<TransactionDTO> GetByIdAsync(int id);
        Task<IEnumerable<TransactionDTO>> GetAllAsync();
        Task<PaginatedResult<TransactionDTO>> GetPaginatedTransactionsAsync(int pageNumber, int pageSize, string? type, int? agencyId, decimal? minValue, decimal? maxValue, DateTime? startDate, DateTime? endDate);

    }
}