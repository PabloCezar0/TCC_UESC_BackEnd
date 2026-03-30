using Rota.Application.DTOs;
using Rota.Domain.Common;

namespace Rota.Application.Interfaces
{
    public interface IMonthlyCommissionService
    {
        Task<PaginatedResult<MonthlyCommissionDTO>> GetPaginatedCommissionsAsync(int pageNumber, int pageSize, int? agencyId, int? year, int? month);
        Task<MonthlyCommissionDTO> GetByIdAsync(int id);
    }
}