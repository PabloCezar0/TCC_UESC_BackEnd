using Microsoft.EntityFrameworkCore;
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using Rota.Infra.Data;
using Rota.Domain.Common;

namespace Rota.Infrastructure.Persistence
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int transactionIdApi, TransactionType type, int agencyId)
        {
            return await _context.Transactions.AnyAsync(c =>
                c.TransactionIdApi == transactionIdApi &&
                c.Type == type &&
                c.AgencyId == agencyId);
        }

        public async Task RemoveAsync(int id)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(c => c.Agency)
                .Include(c => c.User) 
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {

            return await _context.Transactions
                .Include(t => t.Agency)
                .Include(t => t.User) 
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task UpdateAsync(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            return _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByAgencyAndPeriodAsync(int agencyId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(c => c.AgencyId == agencyId && c.Date >= startDate && c.Date <= endDate)
                .ToListAsync();
        }

        public async Task<PaginatedResult<Transaction>> GetPaginatedAsync(
            int page, int size, string? type, int? agencyId, 
            decimal? minValue, decimal? maxValue, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(t => t.Agency)
                .Include(t => t.User) 
                .Where(t => t.DeletedAt == null)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
            {
                query = query.Where(c => c.Type == parsedType);
            }

            if (agencyId.HasValue)
            {
                query = query.Where(c => c.AgencyId == agencyId.Value);
            }

            if (minValue.HasValue) query = query.Where(c => c.Value >= minValue.Value);
            if (maxValue.HasValue) query = query.Where(c => c.Value <= maxValue.Value);
            if (startDate.HasValue) query = query.Where(c => c.Date >= startDate.Value);
            if (endDate.HasValue) query = query.Where(c => c.Date <= endDate.Value);


            query = query.OrderByDescending(t => t.Date);


            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size)
                                   .Take(size)
                                   .ToListAsync();

            return new PaginatedResult<Transaction>
            {
                PageNumber = page,
                PageSize = size,
                TotalItems = total,
                Items = items
            };
        }

        public async Task<IEnumerable<Transaction>> GetByAgencyAsync(int agencyId)
        {
            return await _context.Transactions
                .Include(c => c.Agency)
                .Include(c => c.User)
                .Where(t => t.AgencyId == agencyId) 
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
    }
}