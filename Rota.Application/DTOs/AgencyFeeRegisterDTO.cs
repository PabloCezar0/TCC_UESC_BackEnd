namespace Rota.Application.DTOs
{
    public class AgencyFeesRegisterDTO
    {
        public double FeePackage { get; set; }
        public double FeeTicket { get; set; }
        public double FeeInternet { get; set; }
        public double FeeInsurance { get; set; }
        public double FeeSpecialVolume { get; set; }
        public DateTime FeeRegisteredAt { get; set; }

    }
}
