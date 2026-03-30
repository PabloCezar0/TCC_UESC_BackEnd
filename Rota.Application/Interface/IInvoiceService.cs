using Rota.Domain.Common;
using Rota.Application.DTOs;

namespace Rota.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDTO> UploadAsync(InvoiceUploadDTO dto, CancellationToken ct = default);
    Task<IEnumerable<InvoiceDTO>> GetAllAsync();
    Task<PaginatedResult<InvoiceDTO>> GetPaginatedAsync(int page, int size, CancellationToken ct = default);
    Task<InvoiceDTO> GetByIdAsync(int id);
    Task UpdateAsync(int id, InvoiceUpdateDTO dto);
    Task RemoveAsync(int id);
    Task<InvoiceFileDownloadDTO> DownloadAsync(int invoiceId, CancellationToken ct = default);

}
