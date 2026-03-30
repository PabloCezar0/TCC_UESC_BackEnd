using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Rota.Application.Interfaces;
using Rota.Application.Settings;
using Rota.Domain.Interfaces;

namespace Rota.Application.Services;

public sealed class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository       _users;
    private readonly IEmailService         _email;
    private readonly IEmailTemplateService _tpl;
    private readonly JwtSettings           _jwt;
    private readonly Argon2Hasher          _hasher;   

    public PasswordResetService(
        IUserRepository users,
        IEmailService email,
        IEmailTemplateService tpl,
        IOptions<JwtSettings> jwt,
        Argon2Hasher hasher)
    {
        _users  = users;
        _email  = email;
        _tpl    = tpl;
        _jwt    = jwt.Value;
        _hasher = hasher;
    }

    public async Task RequestResetAsync(string email)
    {
        var user = await _users.GetByEmailAsync(email); 
        if (user is null) return;        

        var token = BuildToken(user);
        var link  = $"{_jwt.ResetUrl}?token={Uri.EscapeDataString(token)}";

        var html = await _tpl.RenderAsync("reset.html",
                    new Dictionary<string,string>
                    {
                        ["Name"]      = user.Name,
                        ["ResetLink"] = link
                    });

        await _email.SendAsync(user.Email, "Redefinição de senha", html);
    }

    public async Task<bool> ConfirmResetAsync(string token, string newPassword)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token,
            _jwt.GetValidationParameters(), out _);

        if (principal.FindFirst("prv")?.Value != "pwd") return false;

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user   = await _users.GetByIdAsync(userId);
        if (user is null) return false;

        user.SetPassword(_hasher.HashPassword(newPassword));
        await _users.UpdateAsync(user);
        return true;
    }

    private string BuildToken(Rota.Domain.Entities.User u)
    {
        var key  = Encoding.ASCII.GetBytes(_jwt.Secret);
        var desc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new Claim(ClaimTypes.Email, u.Email),
                new Claim("prv","pwd")          
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwt.ResetMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        return new JwtSecurityTokenHandler().WriteToken(
                   new JwtSecurityTokenHandler().CreateToken(desc));
    }
}
