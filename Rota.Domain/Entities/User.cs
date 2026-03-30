using Rota.Domain.Validation;
using Rota.Domain.Enums;

namespace Rota.Domain.Entities
{
    public sealed class User : EntityBase
    {
        public string? Name { get; private set; }
        public string? Email { get; private set; }
        public string? Password { get; private set; }
        public UserRole Role { get; private set; }

        public int? AgencyId { get; private set; }
        public Agency? Agency { get; private set; }

        public bool IsActive { get; private set; } = true;

        public User(string name, string email, string password, int? agencyId, UserRole role)
        {
            ValidateDomain(name, email, password);
            AgencyId = agencyId;
            Role = role;
        }

        public User(int id, string name, string email, string password, int? agencyId, UserRole role)
        {
            DomainExceptionValidation.When(id < 0, "ID invalido");
            Id = id;
            ValidateDomain(name, email, password);
            AgencyId = agencyId;
            Role = role;
        }

        public void Update(string name, string email, int? agencyId, UserRole role)
        {
            ValidateDomain(name, email, this.Password!);
            Name = name;
            Email = email;
            AgencyId = agencyId;
            Role = role;
        }

        private void ValidateDomain(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }

        public void SetPassword(string hashedPassword)
        {
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(hashedPassword), "Senha invalida");
            Password = hashedPassword;
        }

        public void SetActive(bool active) => IsActive = active;


    }
}
