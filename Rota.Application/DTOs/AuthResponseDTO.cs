using System;

namespace Rota.Application.DTOs
{
    public class AuthResponseDTO
    {
        public string Token       { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public int    UserId      { get; set; }
        public string Email       { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public int? AgencyId { get; set; }
    }
}
