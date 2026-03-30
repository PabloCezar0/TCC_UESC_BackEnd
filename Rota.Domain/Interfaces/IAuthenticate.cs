using System.Threading.Tasks;
using Rota.Domain.Common;  

namespace Rota.Domain.Interfaces
{
    public interface IAuthenticate
    {
        Task<AuthResult> Authenticate (string email, string password);
        Task<AuthResult> RegisterUser (string email, string password);
        Task Logout();
    }
}
