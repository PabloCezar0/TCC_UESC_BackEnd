using Microsoft.EntityFrameworkCore;
using Rota.Domain.Common;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Infra.Data;                     

namespace Rota.Infrastructure.Persistence;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _ctx;
    public InvoiceRepository(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(Invoice inv)
    { 
        _ctx.Invoices.Add(inv); 
        await _ctx.SaveChangesAsync(); 
    }
    
    public async Task UpdateAsync(Invoice invoice)
    {
        _ctx.Entry(invoice).State = EntityState.Modified;
        await _ctx.SaveChangesAsync();
    }

    public Task<Invoice?> GetByIdAsync(int id) =>
        _ctx.Invoices
            .AsNoTracking()
            .Include(i => i.User)   
            .Include(i => i.Agency) 
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<Invoice>> GetAllAsync() =>
        await _ctx.Invoices
                   .AsNoTracking()
                   .Include(i => i.User)   
                   .Include(i => i.Agency)
                   .OrderByDescending(i => i.IssueDate)
                   .ToListAsync();

        public async Task<PaginatedResult<Invoice>> GetPaginatedAsync(
            int page, int size, 
            int? agencyId, 
            string? userName, 
            CancellationToken ct = default)
        {
            var query = _ctx.Invoices
                            .AsNoTracking()
                            .Include(i => i.User)
                            .Include(i => i.Agency)
                            .AsQueryable(); 

            
            if (agencyId.HasValue)
            {
                query = query.Where(i => i.AgencyId == agencyId.Value);
            }

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(i => i.User.Name.Contains(userName));
            }

            query = query.OrderByDescending(i => i.IssueDate);

            
            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * size)
                                .Take(size)
                                .ToListAsync(ct);

            return new PaginatedResult<Invoice>
            {
                PageNumber = page,
                PageSize   = size,
                TotalItems = total,
                Items      = items
            };
        }

    public async Task RemoveAsync(int id)
    {
        var inv = await _ctx.Invoices.FindAsync(id);
        if (inv == null) return;
        _ctx.Invoices.Remove(inv);
        await _ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByAgencyAndPeriodAsync(int agencyId, int year, int month)
    {
        return await _ctx.Invoices
            .Where(i => i.AgencyId == agencyId && i.ReferenceYear == year && i.ReferenceMonth == month)
            .ToListAsync();
    }
    public async Task<IEnumerable<Invoice>> GetByAgencyAsync(int agencyId)
        {
            return await _ctx.Invoices
                .AsNoTracking() 
                .Include(i => i.Agency) 
                .Include(i => i.User)   
                .Where(i => i.AgencyId == agencyId) 
                .OrderByDescending(i => i.IssueDate) 
                .ToListAsync();
        }
    
     public async Task<Invoice?> GetByIdWithFilesAsync(int id, CancellationToken ct = default) =>
        await _ctx.Invoices
                    .AsNoTracking()
                    .Include(i => i.Files)
                    .Include(i => i.User) 
                    .FirstOrDefaultAsync(i => i.Id == id, ct);
}