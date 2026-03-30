using System;

namespace Rota.Domain.Entities
{
    public sealed class Agency : EntityBase
    {
        public string ExternalId { get; set; } = string.Empty;
        public string CorporateName { get; private set; } = string.Empty;

        public string CNPJ { get; private set; } = string.Empty;

        public string Address { get; private set; } = string.Empty;

        public string AddressNumber { get; private set; } = string.Empty;
        public string AddressComment { get; private set; } = string.Empty;
        public string PhoneNumberOne { get; private set; } = string.Empty;
        public string PhoneNumberTwo { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        public string City { get; private set; } = string.Empty;

        public string State { get; private set; } = string.Empty;

        public double FeePackage { get; private set; }
        public double FeeTicket { get; private set; }
        public double FeeInternet { get; private set; }
        public double FeeInsurance { get; private set; }
        public double FeeSpecialVolume { get; private set; }
        public int? CommissionRuleId { get; private set; }
        public CommissionRule? CommissionRule { get; private set; }

        public DateTime FeeRegisteredAt { get; private set; }

        public Agency() { }

        public Agency(
            string externalId,
            string corporateName,
            string cnpj,
            string address,
            string city,
            string state,
            string? addressNumber,
            string? addressComment,
            string phoneNumberOne,
            string phoneNumberTwo,
            string email)
        {
            ExternalId = externalId ?? string.Empty;
            CorporateName = corporateName ?? string.Empty;
            CNPJ = cnpj ?? string.Empty;
            Address = address ?? string.Empty;
            AddressNumber = addressNumber ?? string.Empty;
            AddressComment = addressComment ?? string.Empty;
            PhoneNumberOne = phoneNumberOne ?? string.Empty;
            PhoneNumberTwo = phoneNumberTwo ?? string.Empty;
            Email = email ?? string.Empty;
            City = city ?? string.Empty;
            State = state ?? string.Empty;
        }

        public void Update(
            string corporateName,
            string cnpj,
            string address,
            string city,
            string state,
            string? addressNumber,
            string? addressComment,
            string phoneNumberOne,
            string phoneNumberTwo,
            string email)
        {
            CorporateName = corporateName ?? string.Empty;
            CNPJ = cnpj ?? string.Empty;
            Address = address ?? string.Empty;
            City = city ?? string.Empty;
            State = state ?? string.Empty;
            AddressNumber = addressNumber ?? string.Empty;
            AddressComment = addressComment ?? string.Empty;
            PhoneNumberOne = phoneNumberOne ?? string.Empty;
            PhoneNumberTwo = phoneNumberTwo ?? string.Empty;
            Email = email ?? string.Empty;
        }

        public void RegisterFees(
            double feePackage,
            double feeTicket,
            double feeInternet,
            double feeInsurance,
            double feeSpecialVolume,
            DateTime feeRegisteredAt)
        {
            FeePackage = feePackage;
            FeeTicket = feeTicket;
            FeeInternet = feeInternet;
            FeeInsurance = feeInsurance;
            FeeSpecialVolume = feeSpecialVolume;
            FeeRegisteredAt = feeRegisteredAt;
        }
        public void SetCommissionRule(int? ruleId)
        {
            CommissionRuleId = ruleId;
        }

    }
}
