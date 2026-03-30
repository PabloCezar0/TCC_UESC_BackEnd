using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rota.Application.Services;        
using Rota.Domain.Common;              
using Rota.Domain.Entities;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using Rota.Infra.Data.Context;

namespace Rota.Infra.Data.Services
{
    public class AuthenticateService : IAuthenticate
    {
        private readonly ApplicationDbContext _ctx;
        private readonly Argon2Hasher         _hasher;
        private readonly IConfiguration       _cfg;

        public AuthenticateService(
            ApplicationDbContext ctx,
            Argon2Hasher hasher,
            IConfiguration cfg)
        {
            _ctx    = ctx;
            _hasher = hasher;
            _cfg    = cfg;
        }


        public async Task<AuthResult> Authenticate(string email, string password)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null || !_hasher.LoginTest(user.Password!, password))
                return AuthResult.Fail("E-mail ou senha inválidos.");

            var token = GenerateToken(user, out DateTime exp);
            return AuthResult.Success(token, exp);
        }

        public async Task<AuthResult> RegisterUser(string email, string password)
        {
            if (await _ctx.Users.AnyAsync(u => u.Email == email))
                return AuthResult.Fail("E-mail já cadastrado.");

            var hash = _hasher.HashPassword(password);

            var agency = await _ctx.Agencies.FirstOrDefaultAsync(a => a.CorporateName == "Matriz");

            if (agency is null)
                return AuthResult.Fail("Agência 'Matriz' não encontrada.");

            var user = new User(
                name:   email.Split('@')[0],
                email:  email,
                password: hash,
                agencyId: agency.Id,
                role:   UserRole.AssistenteAdm);

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            var token = GenerateToken(user, out DateTime exp);
            return AuthResult.Success(token, exp);
        }

        public Task Logout() => Task.CompletedTask;


        private string GenerateToken(User user, out DateTime expires)
        {
            var jwtCfg = _cfg.GetSection("JwtSettings");
            var key    = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtCfg["Secret"]!));

            expires = DateTime.UtcNow.AddMinutes(
                          int.Parse(jwtCfg["ExpiresInMinutes"] ?? "60"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.Role,               user.Role.ToString())
            };

            if (user.AgencyId.HasValue)
            {
                claims.Add(new Claim("AgencyId", user.AgencyId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer:  jwtCfg["Issuer"],
                audience:jwtCfg["Audience"],
                claims:  claims,
                expires: expires,
                signingCredentials:
                    new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
