using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;
using Rota.Domain.Enums;

namespace Rota.Application.DTOs
{
    public class UserUpdateDTO
    {
        public int Id { get; set; }

        [RegularExpression(ValidationRules.NameRegex, ErrorMessage = ValidationRules.NameError)]
        [DisplayName("Nome")]
        public string Name { get; set; }

        [DisplayName("E-mail")]
        public string Email { get; set; }

        [DisplayName("Agência (ID)")]
        public int? AgencyId { get; set; }
        
        [DisplayName("Nome da Agência")]
        public string? AgencyName { get; set; }

        [DisplayName("Perfil")]
        public UserRole Role { get; set; }

        [DisplayName("Ativo")]
        public bool IsActive { get; set; }
        }
}
