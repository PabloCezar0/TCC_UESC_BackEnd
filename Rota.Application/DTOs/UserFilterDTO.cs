using System.Collections.Generic;

namespace Rota.Application.DTOs
{
    public class UserFilterDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Agency { get; set; }
        public List<SortOptionDTO>? Sort { get; set; }
    }

    public class SortOptionDTO
    {
        public string Field { get; set; }
        public string Order { get; set; } 
    }
}