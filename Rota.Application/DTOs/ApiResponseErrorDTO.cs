namespace Rota.Application.DTOs
{
    public class ApiResponseErrorDTO
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; } 
        public List<string>? Errors { get; set; } 
    }
}
