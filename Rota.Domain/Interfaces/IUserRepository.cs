using Rota.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rota.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User>        GetByIdAsync(int id);
        Task<User>        GetByEmailAsync(string email);
        IQueryable<User>  GetQueryable();
        Task<List<User>>  GetPaginatedAsync(IQueryable<User> query, int skip, int take);
        Task<int>         CountAsync(IQueryable<User> query);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task RemoveAsync(int id);         
        Task<IEnumerable<User>> GetDeletedAsync();                  
        Task<User> GetByEmailIncludingDeletedAsync(string email); 
        Task ReactivateAsync(int id);                               
    }
}


