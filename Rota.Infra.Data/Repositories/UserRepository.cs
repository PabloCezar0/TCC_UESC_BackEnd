using Microsoft.EntityFrameworkCore;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Infra.Data.Context;


namespace Rota.Infra.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.Agency)
                                        .ToListAsync();                  
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Agency)
                                        .FirstOrDefaultAsync(u => u.Id == id);                  
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.Agency)
                                        .FirstOrDefaultAsync(u => u.Email == email);
        }

        public IQueryable<User> GetQueryable()
        {
            return _context.Users.Include(u => u.Agency).AsQueryable();
        }

        public async Task<List<User>> GetPaginatedAsync(
            IQueryable<User> query,
            int skip,
            int take)
        {
            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountAsync(IQueryable<User> query)
        {
            return await query.CountAsync();
        }


        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var user = await GetByIdAsync(id);          
            if (user != null)
            {
                user.SoftDelete();    
                user.SetActive(false);                  
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetDeletedAsync()
        {
            return await _context.Users
                                 .IgnoreQueryFilters()   
                                 .Where(u => u.IsDeleted)
                                 .ToListAsync();
        }


        public async Task<User> GetByEmailIncludingDeletedAsync(string email)
        {
            return await _context.Users.Include(u => u.Agency).IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
        }


        public async Task ReactivateAsync(int id)
        {
            var user = await _context.Users
                                     .IgnoreQueryFilters()
                                     .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return;

            user.Restore();         
            user.SetActive(true);            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
