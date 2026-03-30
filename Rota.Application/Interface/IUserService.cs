using Rota.Application.DTOs;
using Rota.Domain.Common;

namespace Rota.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetUsersAsync();
        Task<UserDTO> GetByIdAsync(int id);
        Task AddAsync(UserRegisterDTO userDto);
        Task UpdateAsync(UserUpdateDTO userDto);
        Task RemoveAsync(int id);
        Task<PaginatedResult<UserDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize);
        Task<UserDTO> GetByEmailAsync(string email);

        Task<IEnumerable<UserDTO>> GetDeletedAsync();

        Task<UserDTO> ReactivateAsync(string email);

        Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<UserDTO> SetActivationByEmailAsync(string email, bool active);
        Task<UserDTO?> GetUserForLoginAsync(string email);

      
    }
}