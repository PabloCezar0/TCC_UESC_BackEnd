using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using System.Threading.Tasks;
using Rota.Application.Services; 

namespace Rota.Application.Services
{
    public class CreateAdmin    
    {
        private readonly IUserRepository _userRepository;
        private readonly Argon2Hasher _hasher; 
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateAdmin> _logger;

        public CreateAdmin(
            IUserRepository userRepository, 
            Argon2Hasher hasher, 
            IConfiguration configuration,
            ILogger<CreateAdmin> logger)
        {
            _userRepository = userRepository;
            _hasher = hasher; 
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAdminUserAsync()
        {
            var adminEmail = _configuration["AdminUserSeed:Email"];
            var adminPassword = _configuration["AdminUserSeed:Password"];
            var adminName = _configuration["AdminUserSeed:Name"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogError("Configurações do usuário admin (AdminUserSeed) não encontradas.");
                return;
            }

            var existingUser = await _userRepository.GetByEmailIncludingDeletedAsync(adminEmail);

            if (existingUser == null)
            {
                _logger.LogInformation("Usuário admin não encontrado. Criando um novo...");
                var hashedPassword = _hasher.HashPassword(adminPassword);
                
                
                var adminUser = new User(
                    0, 
                    name: adminName,
                    email: adminEmail,
                    password: hashedPassword,
                    role: UserRole.Administrador,
                    agencyId: null 
                );
                
                await _userRepository.AddAsync(adminUser);
                
                _logger.LogInformation("Usuário admin criado com sucesso.");
            }
            else
            {
                _logger.LogInformation("Usuário admin já existe no banco de dados. Nenhuma ação necessária.");
            }
        }
    }
}