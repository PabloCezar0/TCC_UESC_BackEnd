namespace Rota.Domain.Common
{

    public record AuthResult(
        bool      IsSuccess,
        string?   Token,
        DateTime? ExpiresAt,
        string?   Error)
    {
        public static AuthResult Success(string token, DateTime exp) =>
            new(true, token, exp, null);

        public static AuthResult Fail(string error) =>
            new(false, null, null, error);
    }
}
