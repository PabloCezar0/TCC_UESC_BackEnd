using Rota.Domain.Entities;
using Rota.Domain.Common;

namespace Rota.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice);
    Task<Invoice?> GetByIdAsync(int id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<PaginatedResult<Invoice>> GetPaginatedAsync(int page, int size, int? agencyId, string? userName, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice); 
    Task<IEnumerable<Invoice>> GetByAgencyAsync(int agencyId);
    Task RemoveAsync(int id);
    Task<Invoice?> GetByIdWithFilesAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Invoice>> GetByAgencyAndPeriodAsync(int agencyId, int year, int month);
}
