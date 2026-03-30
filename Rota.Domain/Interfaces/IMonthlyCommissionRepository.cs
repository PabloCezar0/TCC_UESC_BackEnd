using Rota.Domain.Common;
using Rota.Domain.Entities;

namespace Rota.Domain.Interfaces
{
    public interface IMonthlyCommissionRepository
    {

        Task AddOrUpdateAsync(MonthlyCommission monthlyCommission);


        Task<MonthlyCommission?> GetByAgencyAndPeriodAsync(int agencyId, int year, int month);


        Task<MonthlyCommission?> GetByIdAsync(int id);


        Task<PaginatedResult<MonthlyCommission>> GetPaginatedAsync(int pageNumber, int pageSize, int? agencyId, int? year, int? month);
    }
}