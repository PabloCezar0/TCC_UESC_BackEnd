using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Common;

namespace Rota.Domain.Interfaces
{
    public interface ITransactionRepository 
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task AddAsync(Transaction transaction); 
        Task UpdateAsync(Transaction transaction); 
        Task<IEnumerable<Transaction>> GetByAgencyAndPeriodAsync(int agencyId, DateTime startDate, DateTime endDate);
        Task RemoveAsync(int id);

        Task<IEnumerable<Transaction>> GetByAgencyAsync(int agencyId);


        Task<bool> ExistsAsync(int transactionIdApi, TransactionType type, int agencyId);

        Task<PaginatedResult<Transaction>> GetPaginatedAsync(
            int page, 
            int size, 
            string? type = null, 
            int? agencyId = null, 
            decimal? minValue = null, 
            decimal? maxValue = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
        
    }
}