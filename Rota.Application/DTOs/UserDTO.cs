using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;
using Rota.Domain.Enums;

namespace Rota.Application.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }

        [RegularExpression(ValidationRules.NameRegex, ErrorMessage = ValidationRules.NameError)]
        [DisplayName("Nome")]
        public required string Name { get; set; }

        [DisplayName("E-mail")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(ValidationRules.MinPasswordLength, ErrorMessage = ValidationRules.MinPasswordLengthError)]
        [RegularExpression(ValidationRules.StrongPasswordRegex,
        ErrorMessage = ValidationRules.StrongPasswordError)]
        [DataType(DataType.Password)]
        [DisplayName("Senha")]
        public string? Password { get; set; }

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
