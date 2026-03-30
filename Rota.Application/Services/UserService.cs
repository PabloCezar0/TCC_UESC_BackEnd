using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Application.Settings;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Rota.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Rota.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;
        private readonly Argon2Hasher _hasher;
        private readonly IEmailService _email;
        private readonly IEmailTemplateService _tpl;
        private readonly JwtSettings _jwt;
        private readonly IAgencyRepository _agencies;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository users,
            IMapper mapper,
            Argon2Hasher hasher,
            IEmailService email,
            IEmailTemplateService tpl,
            IOptions<JwtSettings> jwt,
            IAgencyRepository agencies,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _tpl = tpl ?? throw new ArgumentNullException(nameof(tpl));
            _jwt = jwt?.Value ?? throw new ArgumentNullException(nameof(jwt));
            _agencies = agencies;
            _httpContextAccessor = httpContextAccessor;
        }


        private bool IsAdmin()
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Administrador"; 
        }
        private int GetCurrentUserId()
        {
            var idStr = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : 0;
        }
        private void ValidateAdminAccess()
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem gerenciar usuários.");
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            ValidateAdminAccess();
            var list = await _users.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<UserDTO>>(list);
            foreach (var d in dtos) d.Password = null;
            return dtos;
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            if (!IsAdmin() && GetCurrentUserId() != id)
            {
                 throw new UnauthorizedAccessException("Acesso negado.");
            }
            var u = await _users.GetByIdAsync(id)
                    ?? throw new KeyNotFoundException($"Usuário {id} não encontrado");
            var dto = _mapper.Map<UserDTO>(u);
            dto.Password = null;
            return dto;
        }

        public async Task<UserDTO> GetByEmailAsync(string email)
        {
            ValidateAdminAccess();
            var u = await _users.GetByEmailAsync(email.Trim().ToLowerInvariant())
                    ?? throw new KeyNotFoundException($"Usuário {email} não encontrado");
            var dto = _mapper.Map<UserDTO>(u);
            dto.Password = null;
            return dto;
        }


        public async Task AddAsync(UserRegisterDTO dto)
        {
            ValidateAdminAccess();
            var email = dto.Email?.Trim().ToLowerInvariant()
                    ?? throw new ArgumentException("E‑mail obrigatório.");

            if (await _users.GetByEmailAsync(email) is { IsDeleted: false })
                throw new ArgumentException($"O e‑mail '{dto.Email}' já está cadastrado.");

            if (dto.AgencyId == null)
                throw new ArgumentException("Agência é obrigatória.");

            var agency = await _agencies.GetByIdAsync(dto.AgencyId.Value);
            if (agency == null)
                throw new KeyNotFoundException($"Agência {dto.AgencyId} não encontrada.");

            dto.Email = email;

            var user = _mapper.Map<User>(dto);
            await _users.AddAsync(user);

            var token = BuildActivationToken(user);
            var link = $"{_jwt.ResetUrl}?token={Uri.EscapeDataString(token)}";

            var html = await _tpl.RenderAsync("welcome.html", new Dictionary<string, string>
            {
                ["Name"] = user.Name,
                ["ActivationLink"] = link
            });

            await _email.SendAsync(user.Email, "Bem‑vindo(a) ao Rota", html);
        }



        public async Task UpdateAsync(UserUpdateDTO dto)
        {
            ValidateAdminAccess();
            var u = await _users.GetByIdAsync(dto.Id)
                    ?? throw new KeyNotFoundException($"Usuário {dto.Id} não encontrado");

            var newEmail = dto.Email.Trim().ToLowerInvariant();

            if (!newEmail.Equals(u.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _users.GetByEmailAsync(newEmail) is { Id: var otherId, IsDeleted: false } && otherId != u.Id)
                    throw new ArgumentException($"O e‑mail '{dto.Email}' já está em uso.");
            }

            u.Update(dto.Name, newEmail, dto.AgencyId, dto.Role);

            await _users.UpdateAsync(u);
        }


        public async Task RemoveAsync(int id)
        {
            ValidateAdminAccess();
            var u = await _users.GetByIdAsync(id)
                    ?? throw new KeyNotFoundException($"Usuário {id} não encontrado");
            await _users.RemoveAsync(id);
        }


        public async Task<bool> LoginTestAsync(string email, string password)
        {
            var u = await _users.GetByEmailAsync(email.Trim().ToLowerInvariant());
            return u is not null
            && !u.IsDeleted
            && u.IsActive
            && !string.IsNullOrWhiteSpace(u.Password)
            && _hasher.LoginTest(u.Password, password);
        }


        public async Task<PaginatedResult<UserDTO>> GetPaginatedUsersAsync(int page, int size)
        {   
            ValidateAdminAccess();
            if (page < 1) throw new ArgumentException("page >= 1");

            var users = await _users.GetAllAsync();
            var total = users.Count();

            var slice = users
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();
            var dtos = _mapper.Map<List<UserDTO>>(slice);
            dtos.ForEach(d => d.Password = null);

            return new PaginatedResult<UserDTO>
            {
                TotalItems = total,
                PageNumber = page,
                PageSize = size,
                Items = dtos
            };
        }



        private string BuildActivationToken(User u)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var desc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
                    new Claim(ClaimTypes.Email, u.Email),
                    new Claim("prv", "pwd")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwt.ResetMinutes),
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(desc));
        }

        public async Task<IEnumerable<UserDTO>> GetDeletedAsync()
        {
            ValidateAdminAccess();
            var list = await _users.GetDeletedAsync();
            var dtos = _mapper.Map<IEnumerable<UserDTO>>(list);
            foreach (var d in dtos) d.Password = null;
            return dtos;
        }
        public async Task<UserDTO> ReactivateAsync(string email)
        {
            ValidateAdminAccess();
            email = email.Trim().ToLowerInvariant();

            var dead = await _users.GetByEmailIncludingDeletedAsync(email)
                      ?? throw new KeyNotFoundException($"Usuário {email} não existe.");

            if (!dead.IsDeleted)
                throw new InvalidOperationException($"Usuário {email} já está ativo.");

            if (await _users.GetByEmailAsync(email) is { IsDeleted: false })
                throw new InvalidOperationException($"Já há um usuário ativo com o e‑mail {email}.");

            await _users.ReactivateAsync(dead.Id);
            dead.Restore();

            var dto = _mapper.Map<UserDTO>(dead);
            dto.Password = null;
            return dto;
        }

        public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            if (!IsAdmin() && GetCurrentUserId() != userId)
                 throw new UnauthorizedAccessException("Acesso negado.");

            var user = await _users.GetByIdAsync(userId)
                    ?? throw new KeyNotFoundException($"Usuário {userId} não encontrado");

            if (string.IsNullOrWhiteSpace(user.Password) ||
                !_hasher.LoginTest(user.Password, oldPassword))
            {
                throw new ArgumentException("Senha antiga incorreta.");
            }
            var hashed = _hasher.HashPassword(newPassword);
            user.SetPassword(hashed);

            await _users.UpdateAsync(user);
        }

        public async Task<UserDTO> SetActivationByEmailAsync(string email, bool active)
        {
            ValidateAdminAccess();
            email = email.Trim().ToLowerInvariant();

            var any = await _users.GetByEmailIncludingDeletedAsync(email)
                    ?? throw new KeyNotFoundException($"Usuário {email} não existe.");

            if (any.IsDeleted)
                throw new InvalidOperationException("Usuário está excluído (soft delete). Reative antes de alterar a ativação.");

            var u = await _users.GetByEmailAsync(email)
                    ?? throw new KeyNotFoundException($"Usuário {email} não encontrado.");

            u.SetActive(active);
            await _users.UpdateAsync(u);

            var dto = _mapper.Map<UserDTO>(u);
            dto.Password = null;
            return dto;
        }
        public async Task<UserDTO?> GetUserForLoginAsync(string email)
        {
            
            var u = await _users.GetByEmailAsync(email.Trim().ToLowerInvariant());
            
            if (u == null) return null;

            var dto = _mapper.Map<UserDTO>(u);
            dto.Password = null; 
            return dto;
        }

    }
}
