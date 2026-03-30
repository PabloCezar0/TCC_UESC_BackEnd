using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Rota.Application.Settings;


public class JwtSettings
{
    public string Secret { get; set; } = default!;


    public int ExpirationMin { get; set; } = 60;

    public int ResetMinutes { get; set; } = 30;


    public string ResetUrl { get; set; } = default!;

    public TokenValidationParameters GetValidationParameters() =>
        new()
        {
            ValidateIssuer           = false,
            ValidateAudience         = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                            Encoding.ASCII.GetBytes(Secret)),
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromMinutes(1)
        };
}
