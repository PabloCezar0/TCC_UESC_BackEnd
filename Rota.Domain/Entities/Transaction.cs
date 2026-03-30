using Rota.Domain.Enums;
using Rota.Domain.Validation;

namespace Rota.Domain.Entities
{
    public sealed class Transaction : EntityBase 
    {
        public TransactionType Type { get; private set; } 
        public decimal Value { get; private set; }
        public DateTime Date { get; private set; }
        public int TransactionIdApi { get; private set; }

        public int AgencyId { get; private set; }
        public Agency Agency { get; private set; } = null!;
        public bool IsManual { get; private set; }

        public int? UserId { get; private set; } 
        public User? User { get; private set; }
        

        private Transaction() { }

        private Transaction(TransactionType type, decimal value, DateTime date, int transactionIdApi, int agencyId, bool isManual, int? userId)
        {
            ValidateDomain(value, date, transactionIdApi, agencyId);
            Type = type;
            Value = value;
            Date = date;
            TransactionIdApi = transactionIdApi;
            AgencyId = agencyId;
            IsManual = isManual;
            UserId = userId;
        }

        public static Transaction CreateAutomatic(TransactionType type, decimal value, DateTime date, int transactionIdApi, int agencyId)
        {
            return new Transaction(type, value, date, transactionIdApi, agencyId, false, null);
        }

        public static Transaction CreateManual(TransactionType type, decimal value, DateTime date, int agencyId, int userId)
        {
            DomainExceptionValidation.When(
                type != TransactionType.Seguro && type != TransactionType.Volume,
                "Para transações manuais, o tipo deve ser 'Seguro' ou 'VolumeEspecial'."
            );

            return new Transaction(type, value, date, 0, agencyId, true, userId);
        }
        public void Update(TransactionType type, decimal value, DateTime date, int transactionIdApi, int agencyId)
        {
            ValidateDomain(value, date, transactionIdApi, agencyId);

            Type = type;
            Value = value;
            Date = date;
            TransactionIdApi = transactionIdApi;
            AgencyId = agencyId;
            IsManual = true;
        }


        private void ValidateDomain(decimal value, DateTime date, int transactionIdApi, int agencyId)
        {
            DomainExceptionValidation.When(value < 0, "O valor da transação não pode ser negativo.");
            DomainExceptionValidation.When(transactionIdApi < 0, "O ID da transação da API é inválido.");
            DomainExceptionValidation.When(agencyId <= 0, "O ID da agência é inválido.");

            Value = value;
            Date = date;
            TransactionIdApi = transactionIdApi;
            AgencyId = agencyId;
        }
    }
}