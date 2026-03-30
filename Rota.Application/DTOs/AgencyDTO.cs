namespace Rota.Application.DTOs
{
    public class AgencyDTO
    {
        public int Id { get; set; }

        public string ExternalId { get; set; } = string.Empty;
        public string CorporateName { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string AddressNumber { get; set; } = string.Empty;
        public string AddressComment { get; set; } = string.Empty;
        public string PhoneNumberOne { get; set; } = string.Empty;
        public string PhoneNumberTwo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public double FeePackage { get; set; }
        public double FeeTicket { get; set; }
        public double FeeInternet { get; set; }
        public double FeeInsurance { get; set; }
        public double FeeSpecialVolume { get; set; }
        public DateTime FeeRegisteredAt { get; set; }


    }
}
